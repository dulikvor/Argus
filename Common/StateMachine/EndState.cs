using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;

namespace Argus.Common.StateMachine
{
    public class EndState<TTransition, TStepInput> : State<TTransition, TStepInput, StepResult>
        where TTransition : Enum
        where TStepInput : StepInput
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => "EndState";

        public EndState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory)
            : base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(StepResult, TTransition)> HandleState(
            StateContext<TTransition, TStepInput, StepResult> context,
            Session<TTransition, TStepInput, StepResult> session,
            TTransition transition,
            TStepInput stepInput)
        {
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(EndPromptDescriptor));

            var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.Keys.EndState));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query(coPilotChatRequestMessage);

            return new(
                    new StepResult
                    {
                        StepSuccess = true,
                        CoPilotChatResponseMessages = chatCompletionResponse
                    },
                    default
                );
        }
    }
}
