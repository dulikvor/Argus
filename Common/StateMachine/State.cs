using Argus.Common.PromptHandlers;

namespace Argus.Common.StateMachine;

public abstract class State<TTransition, TStepInput, TStepResult> where TTransition : Enum
{
    protected IPromptHandlerFactory _promptHandlerFactory;

    protected State(IPromptHandlerFactory promptHandlerFactory)
    {
        _promptHandlerFactory = promptHandlerFactory;
    }

    public virtual Task<(TStepResult, TTransition)> HandleState(StateContext<TTransition, TStepInput, TStepResult> context, TTransition command, TStepInput stepInput)
    {
        throw new InvalidOperationException();
    }
}