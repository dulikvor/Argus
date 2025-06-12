using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using System.Net;
using System.Text;
using System.Text.Json;

using BuiltInPromptsConstants = Argus.Common.Builtin.PromptDescriptor.PromptsConstants;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandInvocationState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandInvocationState);

        public CommandInvocationState(
            IAzureLLMQueryClient llmQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory,
            ISemanticStore semanticStore,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger)
            : base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(CommandInvocationState));
            return await activityScope.Monitor(async () =>
            {
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
            });
        }

        private async Task<(StepResult, ApiTestStateTransitions)> CommandAnalysis(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(CommandInvocationState));
            return await activityScope.Monitor(async () =>
            {
                var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                if (action == ConsentAction.ConsentApproval && isConsentGiven)
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletion,
                        new EndState<ApiTestStateTransitions, StepInput>(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                        ApiTestStateTransitions.Any);
                }

                var (stepResult, apiTestStateTransitions) = await DetectAndTransitionNextState(context, session, stepInput);
                if (apiTestStateTransitions != ApiTestStateTransitions.CommandInvocationAnalysis)
                {
                    return (stepResult, apiTestStateTransitions);
                }


                var chatCompletionResponse = await QueryLLM<CommandInvocationAnalysisOutput>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(CommandInvocationPromptDescriptor),
                    PromptsConstants.CommandInvocation.Keys.CommandInvocationAnalysisPromptKey,
                    PromptsConstants.CommandInvocation.Keys.CommandInvocationAnalysisReturnedOutputKey,
                    null);

                session.ResetStepResult(new(GetName(), Session<ApiTestStateTransitions, StepInput>.IncrementalResultKeyPostfix));

                var structuredOutput = chatCompletionResponse.StructuredOutput;

                return DetectAndConfirm(
                        session,
                        stepInput,
                        chatCompletionResponse,
                        output => true,
                        output => output.InstructionsToUserOnDetected(),
                        ApiTestStateTransitions.CommandInvocationAnalysis,
                        true,
                        false);
            });
        }

        private async Task<(StepResult, ApiTestStateTransitions)> CommandInvocation(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(CommandInvocationState));
            return await activityScope.Monitor<(StepResult, ApiTestStateTransitions)>(async () =>
            {
                var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<(HttpStatusCode HttpStatusCode, string Content)>, string, string, Dictionary<string, string>, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(RestToolFunctionDescriptor));

                var chatCompletionResponse = await QueryLLM<StringResponse>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(CommandInvocationPromptDescriptor),
                    PromptsConstants.CommandInvocation.Keys.CommandInvocationPromptKey,
                    BuiltInPromptsConstants.StructuredResponses.Keys.StringResponseSchema,
                    new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

                var arguments = concreteFunctionDescriptor.GetParameters<RestToolFunctionDescriptor.RestToolParametersType>(JsonSerializer.Serialize(chatCompletionResponse.FunctionResponses.First().FunctionArguments));

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

                AddCommandResultToSession(httpStatus, content, (ApiTestSession)session);

                activityScope.Activity.SetTag("httpStatusCode", httpStatus.ToString());
                activityScope.Activity.SetTag("HttpResponse", content);

                session.SetCurrentStep(this, ApiTestStateTransitions.CommandInvocationAnalysis);
                return new (
                    new StepResult
                    {
                        StepSuccess = true,
                    },
                    ApiTestStateTransitions.CommandInvocationAnalysis);
            });
        }

        private void AddCommandResultToSession(HttpStatusCode httpStatus, string content, ApiTestSession session)
        {
            var sb = new StringBuilder();
            sb.Append($"Command Invocation Result:\n");
            sb.AppendLine($"HTTP Status: {httpStatus}\n");
            sb.AppendLine($"Content: {content}");
            sb.AppendLine();
            session.AddStepResult(new(GetName(), Session<ApiTestStateTransitions, StepInput>.IncrementalResultKeyPostfix), sb.ToString());
        }

        public async Task<(StepResult, ApiTestStateTransitions)> DetectAndTransitionNextState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(CommandInvocationState));
            return await activityScope.Monitor(async () =>
            {
                var chatCompletionResponse = await QueryLLM<CommandInvocationDetectNextStateOutput>(
                stepInput.CoPilotChatRequestMessage,
                nameof(CommandInvocationPromptDescriptor),
                PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStatePromptKey,
                PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStateOutputKey,
                null);

                var output = chatCompletionResponse.StructuredOutput;
                if (output.NextState == "ExpectedOutcomeSelect")
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletionResponse.ChatCompletion,
                        new ExpectedOutcomeState(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                        ApiTestStateTransitions.ExpectedOutcome);
                }
                else if (output.NextState == "CommandSelect")
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletionResponse.ChatCompletion,
                        new CommandSelectState(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                        ApiTestStateTransitions.CommandDiscovery);
                }
                else if (output.NextState == "CommandInvocation")
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletionResponse.ChatCompletion,
                        this,
                        ApiTestStateTransitions.CommandInvocation);
                }
                // If 'None', stay in current analysis state

                return (
                    new StepResult
                    {
                        StepSuccess = true,
                    },
                    ApiTestStateTransitions.CommandInvocationAnalysis
                );
            });
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