using Argus.Common.PromptHandlers;

namespace Argus.Common.StateMachine;

public abstract class State<TEdge, TStepInput, TStepResult> where TEdge : Enum
{
    protected IPromptHandlerFactory _promptHandlerFactory;

    protected State(IPromptHandlerFactory promptHandlerFactory)
    {
        _promptHandlerFactory = promptHandlerFactory;
    }

    public virtual Task<TStepResult> HandleState(StateContext<TEdge, TStepInput, TStepResult> context, TEdge command, TStepInput stepInput)
    {
        throw new InvalidOperationException();
    }
}