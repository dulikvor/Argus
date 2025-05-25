using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;

namespace Argus.Common.StateMachine
{
    public class EndState<TTransition, TStepInput> : State<TTransition, TStepInput>
        where TTransition : Enum
        where TStepInput : StepInput
    {
        public override string GetName() => "EndState";

        public EndState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory,
            ISemanticStore semanticStore)
            : base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, gitHubLLMQueryClient)
        {
        }

        public override async Task<(StepResult, TTransition)> HandleState(
            StateContext<TTransition, TStepInput> context,
            Session<TTransition, TStepInput> session,
            TTransition transition,
            TStepInput stepInput)
        {
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(EndPromptDescriptor));

            var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.Prompts.Keys.EndState));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query(coPilotChatRequestMessage);

            return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = chatCompletionResponse
                    },
                    default
                );
        }
    }
}
