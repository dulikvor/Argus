using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
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
using System.Text;
using System.Text.Json;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class RestDiscoveryState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(RestDiscoveryState);

        public RestDiscoveryState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory, 
            ISemanticStore semanticStore)
            :base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, gitHubLLMQueryClient)
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
            switch (transition)
            {
                case ApiTestStateTransitions.RestDiscovery:
                    {
                        return await RestDiscovery(context, session, stepInput);
                    }
                case ApiTestStateTransitions.RawContentGet:
                    {
                        return await GetRawContent(context, session, stepInput);
                    }
                default:
                    {
                        context.OnNonSupportedTransition(transition);
                        return default;
                    }
            }
        }

        private async Task<(StepResult, ApiTestStateTransitions)> GetRawContent(
            StateContext<ApiTestStateTransitions, StepInput> context, 
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<string>, string, string, string, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var arguments = concreteFunctionDescriptor.GetParameters<GetGitHubRawContentFunctionDescriptor.GetGitHubRawContentParametersType>(JsonSerializer.Serialize(stepInput.PreviousStepResult.FunctionResponses.First().FunctionArguments));

            string rawContent = default;
            var toolArguments = $"{arguments.User}/{arguments.Repo}/{arguments.Branch}/{arguments.PathToFile}";
            try
            {
                rawContent = await concreteFunctionDescriptor.Function(arguments.User, arguments.Repo, arguments.Branch, arguments.PathToFile);
            }
            catch (HttpResponseException exception)
            {
                var errorMessage = $"route used {toolArguments}, returned status code {exception.StatusCode}";
                session.SetCurrentStep(this, ApiTestStateTransitions.RestDiscovery);
                return new(
                 new StepResult
                 {
                     StepSuccess = false,
                     CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                     {
                         new CoPilotChatResponseMessage($"The GetRawContent function failed to execute. {errorMessage}", stepInput.PreviousStepResult.PreviousChatCompletion, false)
                     }
                 },
                 ApiTestStateTransitions.RestDiscovery);
            }

            var inputText = stepInput.CoPilotChatRequestMessage.GetUserFirstAsPlainText();
            var sb = new StringBuilder();
            sb.AppendLine($"Function called: {concreteFunctionDescriptor.ToolDefinition.FunctionName}");
            sb.AppendLine($"Function arguments: {toolArguments}");
            sb.AppendLine($"Function Result: {rawContent}");

            _semanticStore.Add(inputText, sb.ToString());

            session.SetCurrentStep(this, ApiTestStateTransitions.RestDiscovery);
            return new(
                    new StepResult
                    {
                        StepSuccess = true,
                    },
                    ApiTestStateTransitions.RestDiscovery);
        }

        private async Task<(StepResult, ApiTestStateTransitions)> RestDiscovery(
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
                    new CommandInvocationState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                    ApiTestStateTransitions.CommandInvocationAnalysis);
            }

            var concreteFunctionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var chatCompletionResponse = await QueryLLM<RestDiscoveryOutput>(
                stepInput.CoPilotChatRequestMessage,
                nameof(RestDiscoveryPromptDescriptor),
                PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey, 
                PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey,
                new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

            if(chatCompletionResponse.IsToolCall)
            {
                return new(
                    new StepResult
                    {
                        StepSuccess = true,
                        FunctionResponses = chatCompletionResponse.FunctionResponses,
                        PreviousChatCompletion = chatCompletionResponse.ChatCompletion
                    },
                    ApiTestStateTransitions.RawContentGet
                );
            }
            else
            {
                var structuredOutput = chatCompletionResponse.StructuredOutput;
                var sessionResources = (session as ApiTestSession).DetectedResources;
                if (structuredOutput.RestDiscoveryDetectedInCurrentIteration)
                {
                    // Update session on iteration only if results are detected
                    if (sessionResources != null && structuredOutput.DetectedResources.Any())
                    {
                        sessionResources.MergeOrUpdate(structuredOutput.DetectedResources);
                    }
                }

                // Include detected resources in the instructions to the user
                var detectedResourcesSummary = structuredOutput.DetectedResources.Any()
                    ? string.Join("\n", structuredOutput.DetectedResources.Select(r => $"- {r.ResourceDepiction} ({r.HttpMethod})"))
                    : "No resources detected.";

                return DetectAndConfirm(
                    session,
                    stepInput,
                    chatCompletionResponse,
                    output => sessionResources != null && output.DetectedResources.Any(),
                    output => $"The following resources were detected:\n{detectedResourcesSummary}\n\n{output.InstructionsToUserOnDetected()}",
                    ApiTestStateTransitions.RestDiscovery,
                    true);
            }
                
        }
    }
}
