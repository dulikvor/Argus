using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

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
            if (transition == ApiTestStateTransitions.CommandDiscovery)
            {
                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(CommandDiscoveryPromptDescriptor));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.CommandDiscovery.Keys.RestSelectPromptKey));

                var structuredOutput = new OpenAIStructuredOutput(
                    nameof(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey),
                    concretePromptDescriptor.GetStructuredResponse(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey));

                var chatCompletionResponse = await _gitHubLLMQueryClient.Query<CommandDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput, null);
                var commandDiscoveryOutput = chatCompletionResponse.StructuredOutput;

                //if (commandDiscoveryOutput.CommandIsValid)
                //{
                //    if (commandDiscoveryOutput.CommandIsValid)
                //    {
                //        session.AddStepResult(new(GetName(), "SelectedCommand"), commandDiscoveryOutput.SelectedCommand);
                //        session.SetCurrentStep(new EndState<ApiTestStateTransitions, StepInput>(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory), ApiTestStateTransitions.Any);
                //        context.SetState(new EndState<ApiTestStateTransitions, StepInput>(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                //        return new(
                //            new StepResult
                //            {
                //                StepSuccess = true,
                //                CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                //                {
                //                    new CoPilotChatResponseMessage(commandDiscoveryOutput.ToString(), chatCompletionResponse.ChatCompletion, true)
                //                }
                //            },
                //            ApiTestStateTransitions.Any);
                //    }
                //}

                return new (
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                        {
                            new CoPilotChatResponseMessage(commandDiscoveryOutput.ToString(), chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.CommandDiscovery);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}