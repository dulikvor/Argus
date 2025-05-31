using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class ExpectedOutcomeState : State<ApiTestStateTransitions, StepInput>
    {

        public override string GetName() => nameof(ExpectedOutcomeState);

        public ExpectedOutcomeState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory, 
            ISemanticStore semanticStore)
            : base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, gitHubLLMQueryClient)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            if (_isFirstRun)
            {
                return await Introduction(stepInput.CoPilotChatRequestMessage, transition);
            }
            
            if (transition == ApiTestStateTransitions.ExpectedOutcome)
            {
                var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                if (action == ConsentAction.ConsentApproval && isConsentGiven)
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletion,
                        new CommandInvocationState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore),
                        ApiTestStateTransitions.CommandInvocationAnalysis);
                }

                var chatCompletionStructuredResponse = await QueryLLM<ExpectedOutcomeOutput>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(ExpectedOutcomePromptDescriptor),
                    PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomePromptKey,
                    PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey,
                    null);

                return DetectAndConfirm(
                        session,
                        stepInput,
                        chatCompletionStructuredResponse,
                        output => output.IsExpectedDetected,
                        output => output.InstructionsToUserOnDetected(),
                        ApiTestStateTransitions.ExpectedOutcome,
                        true);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}