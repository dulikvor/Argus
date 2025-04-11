namespace Argus.Common.StateMachine;

public abstract class State<TEdge, TArg1, TArg2, TResult> where TEdge : Enum
{
    public virtual TResult HandleState(StateContext<TEdge, TArg1, TArg2, TResult> context, TEdge command, TArg1 argument1, TArg2 argument2)
    {
        throw new InvalidOperationException();
    }
}
