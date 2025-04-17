using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class EndState<TTransition, TStepInput, TStepResult> : State<TTransition, TStepInput, TStepResult> where TTransition : Enum
    {
        public EndState(IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory)
            : base(promptDescriptorFactory, functionDescriptorFactory)
        {
        }
    }
}
