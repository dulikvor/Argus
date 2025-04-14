using Argus.Clients.GitHubLLMQuery;
using Argus.Common.PromptHandlers;
using Argus.Common.StateMachine;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class EndState<TTransition, TStepInput, TStepResult> : State<TTransition, TStepInput, TStepResult> where TTransition : Enum
    {
        public EndState(IPromptHandlerFactory promptHandlerFactory)
            : base(promptHandlerFactory)
        {
        }
    }
}
