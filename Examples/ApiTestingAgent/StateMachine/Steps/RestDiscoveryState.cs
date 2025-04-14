using ApiTestingAgent.StateMachine.PromptHandlers;
using ApiTestingAgent.StateMachine.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.PromptHandlers;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class RestDiscoveryState : State<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult>
    {
        private IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public RestDiscoveryState(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptHandlerFactory promptHandlerFactory)
            :base(promptHandlerFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(ApiTestsStepResult, ApiTestStateTransitions)> HandleState(StateContext<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult> context, ApiTestStateTransitions transition, ApiTestsStepInput stepInput)
        {
            if (transition == ApiTestStateTransitions.RestDiscovery)
            {
                var concretePromptHandler = _promptHandlerFactory.GetPromptHandler(nameof(RestDiscoveryPromptHandler));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptHandler.GetPrompt(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey),
                    concretePromptHandler.GetStructuredResponse(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey));

                var (restDiscovery, chatCompletion) = await _gitHubLLMQueryClient.Query<RestDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput);

                if (restDiscovery.RestDiscoveryIsValid)
                {
                    context.SetState(new EndState<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult>(_promptHandlerFactory));
                }

                return new (
                    new ApiTestsStepResult
                    {
                        StepSuccess = restDiscovery.RestDiscoveryIsValid,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>() { new CoPilotChatResponseMessage(restDiscovery.ToString(), chatCompletion, false) }
                    },
                    ApiTestStateTransitions.RestDiscovery
                    );
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}
