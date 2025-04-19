using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class ServiceInformationState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(ServiceInformationState);

        public ServiceInformationState(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory)
            :base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context, 
            Session<ApiTestStateTransitions, StepInput, StepResult> session, 
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            ApiTestStateTransitions nextTransition = transition;
            if (transition == ApiTestStateTransitions.TestDescriptor)
            {
                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(ServiceInformationPromptDescriptor));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey),
                    concretePromptDescriptor.GetStructuredResponse(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey));

                var chatCompletionResponse = await _gitHubLLMQueryClient.Query<ServiceInformationDomainOutput>(coPilotChatRequestMessage, structuredOutput, null);
                var serviceInformationDomain = chatCompletionResponse.StructuredOutput;
                if (serviceInformationDomain.ServiceDomainIsValid)
                {
                    session.AddStepResult(new(GetName(), "FoundDomain"), serviceInformationDomain.ServiceDomain);
                    context.SetState(new RestDiscoveryState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                    nextTransition = ApiTestStateTransitions.RestDiscovery;
                }

                return new (
                    new StepResult()
                    {
                        StepSuccess = serviceInformationDomain.ServiceDomainIsValid,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>() { new CoPilotChatResponseMessage(serviceInformationDomain.ToString(), chatCompletionResponse.ChatCompletion, serviceInformationDomain.ServiceDomainIsValid) }
                    },
                    nextTransition);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}
