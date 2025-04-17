using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using static ApiTestingAgent.StateMachine.StatePromptsConstants;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class RestDiscoveryState : State<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult>
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(RestDiscoveryState);

        public RestDiscoveryState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory)
            :base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(ApiTestsStepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult> context, 
            Session<ApiTestStateTransitions> session, 
            ApiTestStateTransitions transition, 
            ApiTestsStepInput stepInput)
        {
            switch (transition)
            {
                case ApiTestStateTransitions.RestDiscovery:
                    {
                        return await RestDiscovery(context, session, stepInput.CoPilotChatRequestMessage, stepInput.PreviousStepResult?.AuxiliaryMessages);
                    }
                case ApiTestStateTransitions.RawContentGet:
                    {
                        return await GetRawContent(context, session, stepInput.PreviousStepResult.FunctionResponses.First());
                    }
                default:
                    {
                        context.OnNonSupportedTransition(transition);
                        return default;
                    }
            }
        }

        private async Task<(ApiTestsStepResult, ApiTestStateTransitions)> GetRawContent(StateContext<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult> context, Session<ApiTestStateTransitions> session, FunctionResponse functionResponse)
        {
            var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<string>, string, string, string, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var arguments = concreteFunctionDescriptor.GetParameters<GetGitHubRawContentFunctionDescriptor.GetGitHubRawContentParametersType>(JsonSerializer.Serialize(functionResponse.FunctionArguments));

            string rawContent = default;
            try
            {
                rawContent = await concreteFunctionDescriptor.Function(arguments.User, arguments.Repo, arguments.Branch, arguments.PathToFile);
            }
            catch (HttpResponseException)
            {
                return new(
                 new ApiTestsStepResult
                 {
                     StepSuccess = false,
                 },
                 ApiTestStateTransitions.RestDiscovery);
            }

            return new(
                    new ApiTestsStepResult
                    {
                        StepSuccess = true,
                        AuxiliaryMessages = new List<string> { $"Tool {concreteFunctionDescriptor.ToolDefinition.FunctionName} result: {rawContent}" }
                    },
                    ApiTestStateTransitions.RestDiscovery);
        }

        private async Task<(ApiTestsStepResult, ApiTestStateTransitions)> RestDiscovery(StateContext<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult> context, Session<ApiTestStateTransitions> session, CoPilotChatRequestMessage coPilotChatRequestMessage, IList<string> auxiliaryMessages)
        {
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(RestDiscoveryPromptDescriptor));
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey));
            foreach(var auxiliaryMessage in auxiliaryMessages ?? new List<string>())
            {
                coPilotChatRequestMessage.AddSystemMessage(auxiliaryMessage);
            }

            var structuredOutput = new OpenAIStructuredOutput(
                nameof(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey),
                concretePromptDescriptor.GetStructuredResponse(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey));

            var functionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query<RestDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput, new List<ChatTool> { functionDescriptor.ToolDefinition });

            if(chatCompletionResponse.IsToolCall)
            {
                return new(
                    new ApiTestsStepResult
                    {
                        StepSuccess = true,
                        FunctionResponses = chatCompletionResponse.FunctionResponses
                    },
                    ApiTestStateTransitions.RawContentGet
                );
            }
            else
            {
                var restDiscovery = chatCompletionResponse.StructuredOutput;
                if (restDiscovery.RestDiscoveryIsValid)
                {
                    context.SetState(new EndState<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult>(_promptDescriptorFactory, _functionDescriptorFactory));
                }

                return new(
                    new ApiTestsStepResult
                    {
                        StepSuccess = restDiscovery.RestDiscoveryIsValid,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>() { new CoPilotChatResponseMessage(restDiscovery.ToString(), chatCompletionResponse.ChatCompletion, false) }
                    },
                    ApiTestStateTransitions.TestDescriptor
                );
            }
                
        }
    }
}
