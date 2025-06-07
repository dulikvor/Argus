using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using System.Diagnostics;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class DomainSelectionState : State<ApiTestStateTransitions, StepInput>
    {

        public override string GetName() => nameof(DomainSelectionState);

        public DomainSelectionState(
            IAzureLLMQueryClient llmQueryClient, 
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory, 
            ISemanticStore semanticStore,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger)
            :base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context, 
            Session<ApiTestStateTransitions, StepInput> session, 
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(DomainSelectionState));
            return await activityScope.Monitor(async () =>
            {
                ApiTestStateTransitions nextTransition = transition;
                //if (_isFirstRun)
                //{
                //    return await Introduction(stepInput.CoPilotChatRequestMessage, nextTransition);
                //}
                if (transition == ApiTestStateTransitions.ServiceInformationDiscovery)
                {
                    var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                    if (action == ConsentAction.ConsentApproval && isConsentGiven)
                    {
                        return TransitionToNextState(
                            context,
                            session,
                            chatCompletion,
                            new RestDiscoveryState(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                            ApiTestStateTransitions.RestDiscovery);
                    }

                    var chatCompletionStructuredResponse = await QueryLLM<DomainSelectionOutput>(
                        stepInput.CoPilotChatRequestMessage,
                        nameof(ServiceInformationPromptDescriptor),
                        PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey,
                        PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
                        null);

                    activityScope.Activity.SetTag("UserResponse", chatCompletionStructuredResponse.StructuredOutput.UserResponse);

                    return DetectAndConfirm(
                        session,
                        stepInput,
                        chatCompletionStructuredResponse,
                        output => !string.IsNullOrEmpty(output.DetectedDomain),
                        output => output.UserResponse,
                        ApiTestStateTransitions.ServiceInformationDiscovery,
                        true);
                }

                context.OnNonSupportedTransition(transition);
                return default;
            });
        }
    }
}
