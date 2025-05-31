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
    public class CommandDiscoveryState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(CommandDiscoveryState);

        public CommandDiscoveryState(
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
            if (transition == ApiTestStateTransitions.CommandDiscovery)
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
        }
    }
}