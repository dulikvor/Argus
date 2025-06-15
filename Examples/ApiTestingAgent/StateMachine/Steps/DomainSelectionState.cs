using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.Orchestration;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;
using Argus.Common.Web;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class DomainSelectionState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(DomainSelectionState);

        public DomainSelectionState(
            IOrchestrationService<ApiTestStateTransitions, StepInput> orchestrationService,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory, 
            ISemanticStore semanticStore,
            IAzureLLMQueryClient llmQueryClient,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger,
            StreamReporter streamReporter,
            IStateFactory stateFactory)
            : base(orchestrationService, promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger, streamReporter, stateFactory)
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
                var (isConsentGiven, action, chatCompletion) = await _orchestrationService.CheckCustomerConsent(session, stepInput);
                if (action == ConsentAction.ConsentApproval && isConsentGiven)
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletion,
                        null,
                        null,
                        _stateFactory.Create<RestDiscoveryState, ApiTestStateTransitions, StepInput>(),
                        ApiTestStateTransitions.RestDiscovery);
                }

                var chatCompletionStructuredResponse = await _orchestrationService.QueryLLM<DomainSelectionOutput>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(ServiceInformationPromptDescriptor),
                    PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey,
                    PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
                    null);

                activityScope.Activity.SetTag("UserResponse", chatCompletionStructuredResponse.StructuredOutput.UserResponse);

                return await _orchestrationService.DetectAndConfirm(
                    session,
                    stepInput,
                    GetStepResultKey(),
                    chatCompletionStructuredResponse,
                    output => !string.IsNullOrEmpty(output.DetectedDomain),
                    output => output.UserResponse,
                    ApiTestStateTransitions.ServiceInformationDiscovery,
                    true);
            });
        }
    }
}
