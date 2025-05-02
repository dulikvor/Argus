using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using static ApiTestingAgent.PromptDescriptor.PromptsConstants;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandDiscoveryState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(CommandDiscoveryState);

        public CommandDiscoveryState(
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
            var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
            var previousChatCompletion = stepInput.PreviousStepResult.PreviousChatCompletion;
            if (transition == ApiTestStateTransitions.CommandDiscovery)
            {
                var confirmationState = coPilotChatRequestMessage.GetUserFirst().GetConfirmation(session.CurrentConfirmationId);
                if (confirmationState == ConfirmationState.Accepted)
                {
                    session.ResetConfirmationId();
                    context.SetState(new ExpectedOutcomeState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                    session.SetCurrentStep(context.GetCurrentState(), ApiTestStateTransitions.ExpectedOutcome);
                    return new(
                            new StepResult
                            {
                                StepSuccess = true,
                                CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                                {
                                    new CoPilotChatResponseMessage("Command Selected.", previousChatCompletion, false)
                                }
                            },
                            ApiTestStateTransitions.ExpectedOutcome);
                }

                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(CommandDiscoveryPromptDescriptor));
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.CommandDiscovery.Keys.RestSelectPromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey),
                    concretePromptDescriptor.GetStructuredResponse(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey));

                var chatCompletionResponse = await _gitHubLLMQueryClient.Query<CommandDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput, null);
                var commandDiscoveryOutput = chatCompletionResponse.StructuredOutput;

                if(commandDiscoveryOutput.CommandIsValid)
                {
                    var confirmation = CopilotConfirmationRequestMessage.GenerateConfirmationData();
                    session.SetCurrentConfirmationId(confirmation.Id);
                    return new(
                            new StepResult
                            {
                                StepSuccess = false,
                                ConfirmationMessage = new CopilotConfirmationRequestMessage
                                {
                                    Title = "Confirm Selected Command",
                                    Message = commandDiscoveryOutput.OutputResult(),
                                    Confirmation = confirmation,
                                },
                                PreviousChatCompletion = chatCompletionResponse.ChatCompletion
                            },
                            ApiTestStateTransitions.CommandDiscovery);
                }

                return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                        {
                            new CoPilotChatResponseMessage(commandDiscoveryOutput.OutputIncrementalResult(), chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.CommandDiscovery);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}