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
    public class ExpectedOutcomeState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(ExpectedOutcomeState);

        public ExpectedOutcomeState(
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
            using var activityScope = ActivityScope.Create(nameof(ExpectedOutcomeState));
            return await activityScope.Monitor(async () =>
            {
                if (transition == ApiTestStateTransitions.ExpectedOutcome)
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
                            _stateFactory.Create<CommandInvocationState, ApiTestStateTransitions, StepInput>(),
                            ApiTestStateTransitions.CommandInvocationAnalysis);
                    }

                    var chatCompletionStructuredResponse = await _orchestrationService.QueryLLM<ExpectedOutcomeOutput>(
                        stepInput.CoPilotChatRequestMessage,
                        nameof(ExpectedOutcomePromptDescriptor),
                        PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomePromptKey,
                        PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey,
                        null);

                    return await _orchestrationService.DetectAndConfirm(
                            session,
                            stepInput,
                            GetStepResultKey(),
                            chatCompletionStructuredResponse,
                            output => output.IsExpectedDetected,
                            output => output.InstructionsToUserOnDetected(),
                            ApiTestStateTransitions.ExpectedOutcome,
                            true);
                }

                context.OnNonSupportedTransition(transition);
                return default;
            });
        }
    }
}