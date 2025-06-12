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

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandSelectState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandSelectState);

        public CommandSelectState(
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
            using var activityScope = ActivityScope.Create(nameof(CommandSelectState));
            return await activityScope.Monitor(async () =>
            {
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
                            ApiTestStateTransitions.CommandInvocation);
                    }

                    var apiTestSession = (ApiTestSession)session;
                    var apiTestsSessionPromptDescriptor = (ApiTestsSessionPromptDescriptor)_promptDescriptorFactory.GetPromptDescriptor(nameof(ApiTestsSessionPromptDescriptor));
                    stepInput.CoPilotChatRequestMessage.AddSystemMessage(apiTestsSessionPromptDescriptor.ReconcileDetectedOperationContextPrompt(apiTestSession.Resources), SystemMessagePriority.Medium);

                    var chatCompletionStructuredResponse = await QueryLLM<CommandSelectOutput>(
                        stepInput.CoPilotChatRequestMessage,
                        nameof(CommandSelectPromptDescriptor),
                        PromptsConstants.CommandSelect.Keys.RestSelectPromptKey,
                        PromptsConstants.CommandSelect.Keys.RestSelectReturnedOutputKey,
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