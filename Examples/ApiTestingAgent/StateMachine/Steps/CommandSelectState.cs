using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
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
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandSelectState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandSelectState);

        public CommandSelectState(
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
            using var activityScope = ActivityScope.Create(nameof(CommandSelectState));
            return await activityScope.Monitor(async () =>
            {
                if (transition == ApiTestStateTransitions.CommandSelect)
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
                            ApiTestStateTransitions.CommandInvocation);
                    }

                    var apiTestSession = (ApiTestSession)session;
                    var apiTestsSessionPromptDescriptor = (ApiTestsSessionPromptDescriptor)_promptDescriptorFactory.GetPromptDescriptor(nameof(ApiTestsSessionPromptDescriptor));
                    stepInput.CoPilotChatRequestMessage.AddSystemMessage(apiTestsSessionPromptDescriptor.ReconcileDetectedOperationContextPrompt(apiTestSession.Resources), SystemMessagePriority.Medium);

                    var chatCompletionStructuredResponse = await _orchestrationService.QueryLLM<CommandSelectOutput>(
                        stepInput.CoPilotChatRequestMessage,
                        nameof(CommandSelectPromptDescriptor),
                        PromptsConstants.CommandSelect.Keys.RestSelectPromptKey,
                        PromptsConstants.CommandSelect.Keys.RestSelectReturnedOutputKey,
                        null);

                    return await _orchestrationService.DetectAndConfirm(
                            session,
                            stepInput,
                            GetStepResultKey(),
                            chatCompletionStructuredResponse,
                            output => output.CommandIsValid,
                            output => output.InstructionsToUserOnDetected(),
                            ApiTestStateTransitions.CommandSelect,
                            true);
                }
                

                context.OnNonSupportedTransition(transition);
                return default;
            });
        }
    }
}