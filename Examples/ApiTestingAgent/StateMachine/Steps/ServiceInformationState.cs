using ApiTestingAgent.StateMachine.PromptHandlers;
using ApiTestingAgent.StateMachine.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.PromptHandlers;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class ServiceInformationState : State<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult>
    {
        private IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public ServiceInformationState(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptHandlerFactory promptHandlerFactory)
            :base(promptHandlerFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(ApiTestsStepResult, ApiTestStateTransitions)> HandleState(StateContext<ApiTestStateTransitions, ApiTestsStepInput, ApiTestsStepResult> context, ApiTestStateTransitions transition, ApiTestsStepInput stepInput)
        {
            ApiTestStateTransitions nextTransition = transition;
            if (transition == ApiTestStateTransitions.TestDescriptor)
            {
                var concretePromptHandler = _promptHandlerFactory.GetPromptHandler(nameof(ServiceInformationPromptHandler));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptHandler.GetPrompt(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey),
                    concretePromptHandler.GetStructuredResponse(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey));

                var (serviceInformationDomain, chatCompletion) = await _gitHubLLMQueryClient.Query<ServiceInformationDomainOutput>(coPilotChatRequestMessage, structuredOutput);

                if (serviceInformationDomain.ServiceDomainIsValid)
                {
                    context.SetState(new RestDiscoveryState(_gitHubLLMQueryClient, _promptHandlerFactory));
                    nextTransition = ApiTestStateTransitions.RestDiscovery;
                }

                return new (
                    new ApiTestsStepResult()
                    {
                        StepSuccess = serviceInformationDomain.ServiceDomainIsValid,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>() { new CoPilotChatResponseMessage(serviceInformationDomain.ToString(), chatCompletion, serviceInformationDomain.ServiceDomainIsValid) }
                    },
                    nextTransition);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}
