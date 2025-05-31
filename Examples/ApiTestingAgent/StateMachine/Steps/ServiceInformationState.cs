using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class ServiceInformationState : State<ApiTestStateTransitions, StepInput>
    {

        public override string GetName() => nameof(ServiceInformationState);

        public ServiceInformationState(
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
            ApiTestStateTransitions nextTransition = transition;
            if(_isFirstRun)
            {
                return await Introduction(stepInput.CoPilotChatRequestMessage, nextTransition);
            }
            if (transition == ApiTestStateTransitions.ServiceInformationDiscovery)
            {
                var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                if (action == ConsentAction.ConsentApproval && isConsentGiven)
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletion,
                        new RestDiscoveryState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                        ApiTestStateTransitions.RestDiscovery);
                }

                var chatCompletionStructuredResponse = await QueryLLM<ServiceInformationDomainOutput>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(ServiceInformationPromptDescriptor),
                    PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey,
                    PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
                    null);

                return DetectAndConfirm(
                    session,
                    stepInput,
                    chatCompletionStructuredResponse,
                    output => output.ServiceDomainDetectedInCurrentIteration,
                    output => output.InstructionsToUserOnDomainDetected,
                    ApiTestStateTransitions.ServiceInformationDiscovery,
                    true);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}
