using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandDiscoveryState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandDiscoveryState);

        public CommandDiscoveryState(
            IAzureLLMQueryClient llmQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory,
            ISemanticStore semanticStore,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger)
            : base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(CommandDiscoveryState));
            return await activityScope.Monitor(async () =>
            {
                if (_isFirstRun)
                {
                    return await Introduction(stepInput.CoPilotChatRequestMessage, transition);
                }
                if (transition == ApiTestStateTransitions.CommandDiscovery)
                {
                    var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                    if (action == ConsentAction.ConsentApproval && isConsentGiven)
                    {
                        return TransitionToNextState(
                            context,
                            session,
                            chatCompletion,
                            new CommandInvocationState(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                            ApiTestStateTransitions.CommandInvocationAnalysis);
                    }

                    var chatCompletionStructuredResponse = await QueryLLM<CommandDiscoveryOutput>(
                        stepInput.CoPilotChatRequestMessage,
                        nameof(CommandDiscoveryPromptDescriptor),
                        PromptsConstants.CommandDiscovery.Keys.RestSelectPromptKey,
                        PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey,
                        null);

                    return DetectAndConfirm(
                            session,
                            stepInput,
                            chatCompletionStructuredResponse,
                            output => output.CommandIsValid,
                            output => output.InstructionsToUserOnDetected(),
                            ApiTestStateTransitions.CommandDiscovery,
                            true);
                }
                

                context.OnNonSupportedTransition(transition);
                return default;
            });
        }
    }
}