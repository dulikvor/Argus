using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class ExpectedOutcomeState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(ExpectedOutcomeState);

        public ExpectedOutcomeState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory)
            : base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context,
            Session<ApiTestStateTransitions, StepInput, StepResult> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            if (transition == ApiTestStateTransitions.ExpectedOutcome)
            {
                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(ExpectedOutcomePromptDescriptor));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomePromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey),
                    concretePromptDescriptor.GetStructuredResponse(PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey));

                var chatCompletionResponse = await _gitHubLLMQueryClient.Query<ExpectedOutcomeOutput>(coPilotChatRequestMessage, structuredOutput, null);
                var expectedOutcome = chatCompletionResponse.StructuredOutput;

                if (expectedOutcome.StepIsConcluded)
                {
                    session.AddStepResult(new(GetName(), "ExpectedOutcome"), expectedOutcome.OutputIncrementalResult());
                    context.SetState(new CommandInvocationState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                    session.SetCurrentStep(context.GetCurrentState(), ApiTestStateTransitions.CommandInvocation);
                    return new(
                        new StepResult
                        {
                            StepSuccess = true,
                            CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                            {
                                new CoPilotChatResponseMessage(expectedOutcome.OutputResult(), chatCompletionResponse.ChatCompletion, true)
                            }
                        },
                        ApiTestStateTransitions.CommandInvocation);
                }

                return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                        {
                            new CoPilotChatResponseMessage(expectedOutcome.OutputResult(), chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.ExpectedOutcome);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}