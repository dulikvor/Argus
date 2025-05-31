using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandInvocationState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandInvocationState);

        public CommandInvocationState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory,
            ISemanticStore semanticStore)
            : base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, gitHubLLMQueryClient)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            if (_isFirstRun)
            {
                return await Introduction(stepInput.CoPilotChatRequestMessage, transition);
            }
            if (transition == ApiTestStateTransitions.CommandInvocationAnalysis)
            {
                return await CommandAnalysis(context, session, stepInput);
            }
            if (transition == ApiTestStateTransitions.CommandInvocation)
            {
                return await CommandInvocation(context, session, stepInput);
            }
            context.OnNonSupportedTransition(transition);
            return default;
        }

        private async Task<(StepResult, ApiTestStateTransitions)> CommandAnalysis(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
            if (action == ConsentAction.ConsentApproval && isConsentGiven)
            {
                return TransitionToNextState(
                    context,
                    session,
                    chatCompletion,
                    new EndState<ApiTestStateTransitions, StepInput>(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                    ApiTestStateTransitions.Any);
            }

            var (stepResult, apiTestStateTransitions) = await DetectAndTransitionNextState(context, session, stepInput);
            if(apiTestStateTransitions != ApiTestStateTransitions.CommandInvocationAnalysis)
            {
                return (stepResult, apiTestStateTransitions);
            }

            var concreteFunctionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(RestToolFunctionDescriptor));

            var chatCompletionResponse = await QueryLLM<CommandInvocationOutput>(
                stepInput.CoPilotChatRequestMessage,
                nameof(CommandInvocationPromptDescriptor),
                PromptsConstants.CommandInvocation.Keys.CommandInvocationPromptKey,
                PromptsConstants.CommandInvocation.Keys.CommandInvocationReturnedOutputKey,
                new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

            if (chatCompletionResponse.IsToolCall)
            {
                // If tool call is detected, transition to CommandInvocation
                return new(
                    new StepResult
                    {
                        StepSuccess = true,
                        FunctionResponses = chatCompletionResponse.FunctionResponses,
                        PreviousChatCompletion = chatCompletionResponse.ChatCompletion
                    },
                    ApiTestStateTransitions.CommandInvocation
                );
            }

            var structuredOutput = chatCompletionResponse.StructuredOutput;

            return DetectAndConfirm(
                    session,
                    stepInput,
                    chatCompletionResponse,
                    output => output.IsExpectedDetected,
                    output => output.InstructionsToUserOnDetected(),
                    ApiTestStateTransitions.CommandInvocationAnalysis,
                    true);
        }

        private async Task<(StepResult, ApiTestStateTransitions)> CommandInvocation(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            if (stepInput.PreviousStepResult == null)
            {
                return (
                new StepResult
                {
                    StepSuccess = false,
                },
                ApiTestStateTransitions.CommandInvocationAnalysis
                );
            }

            var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<(HttpStatusCode HttpStatusCode, string Content)>, string, string, Dictionary<string, string>, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(RestToolFunctionDescriptor));
            var arguments = concreteFunctionDescriptor.GetParameters<RestToolFunctionDescriptor.RestToolParametersType>(JsonSerializer.Serialize(stepInput.PreviousStepResult.FunctionResponses.First().FunctionArguments));

            HttpStatusCode httpStatus = default;
            string content = null;
            try
            {
                var response = await concreteFunctionDescriptor.Function(arguments.Method, arguments.Url, arguments.Headers, arguments.Body);
                httpStatus = response.HttpStatusCode;
                content = response.Content;
            }
            catch (HttpResponseException exception)
            {
                httpStatus = exception.StatusCode;
                content = exception.Message;
            }
            catch (Exception ex)
            {
                content = $"An error occurred while invoking the command: {ex.Message}";
            }

            var toolArgumentsDepiction = GetToolArgumentsDepiction(arguments);
            var inputText = stepInput.CoPilotChatRequestMessage.GetUserFirstAsPlainText();
            var sb = new StringBuilder();
            sb.AppendLine($"Function called: {concreteFunctionDescriptor.ToolDefinition.FunctionName}");
            sb.AppendLine($"Function arguments: {toolArgumentsDepiction}");
            sb.AppendLine($"Function Result: HTTP Status: {httpStatus}\nContent: {content}");
            _semanticStore.Add(inputText, sb.ToString());


            var requestMessage = stepInput.CoPilotChatRequestMessage.CreateSingleMessageRequest(sb.ToString());
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(CommandInvocationPromptDescriptor));
            requestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.CommandInvocation.Keys.CommandInvocationHttpResultExplanationPromptKey));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query<string>(requestMessage, null, null);

            session.SetCurrentStep(this, ApiTestStateTransitions.CommandInvocationAnalysis);
            return new(
                new StepResult
                {
                    CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                    {
                        new CoPilotChatResponseMessage(chatCompletionResponse.StructuredOutput, chatCompletionResponse.ChatCompletion, true)
                    },
                    StepSuccess = true,
                },
                ApiTestStateTransitions.CommandInvocationAnalysis);
        }

        public async Task<(StepResult, ApiTestStateTransitions)> DetectAndTransitionNextState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            var chatCompletionResponse = await QueryLLM<CommandInvocationDetectNextStateOutput>(
               stepInput.CoPilotChatRequestMessage,
               nameof(CommandInvocationPromptDescriptor),
               PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStatePromptKey,
               PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStateOutputKey,
               null);

            var output = chatCompletionResponse.StructuredOutput;
            if (output.NextState == "ExpectedOutcome")
            {
                return TransitionToNextState(
                    context,
                    session,
                    chatCompletionResponse.ChatCompletion,
                    new ExpectedOutcomeState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                    ApiTestStateTransitions.ExpectedOutcome);
            }
            else if (output.NextState == "CommandSelect")
            {
                return TransitionToNextState(
                    context,
                    session,
                    chatCompletionResponse.ChatCompletion,
                    new CommandDiscoveryState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                    ApiTestStateTransitions.CommandDiscovery);
            }
            // If 'None', stay in current analysis state

            return (
                new StepResult
                {
                    CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                    {
                        new CoPilotChatResponseMessage(output.CurrentStatus, chatCompletionResponse.ChatCompletion, true)
                    },
                    StepSuccess = true,
                },
                ApiTestStateTransitions.CommandInvocationAnalysis
            );
        }

        // Helper to create a string depicting the arguments for LLM context
        private static string GetToolArgumentsDepiction(RestToolFunctionDescriptor.RestToolParametersType arguments)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Method: {arguments.Method}");
            sb.AppendLine($"URL: {arguments.Url}");
            if (arguments.Headers != null && arguments.Headers.Count > 0)
            {
                sb.AppendLine("Headers:");
                foreach (var header in arguments.Headers)
                {
                    sb.AppendLine($"  {header.Key}: {header.Value}");
                }
            }
            if (!string.IsNullOrEmpty(arguments.Body))
            {
                sb.AppendLine($"Body: {arguments.Body}");
            }
            return sb.ToString();
        }
    }
}