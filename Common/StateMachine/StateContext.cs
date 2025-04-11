namespace Argus.Common.StateMachine;

public class StateContext<TEdge, TArg1, TArg2, TResult> where TEdge : Enum
{
    private State<TEdge, TArg1, TArg2, TResult> _currentState;

    public StateContext(State<TEdge, TArg1, TArg2, TResult> startingState)
    {
        _currentState = startingState;
    }

    public TResult HandleState(TEdge commandType, TArg1 argument1, TArg2 argument2)
    {
        return _currentState.HandleState(this, commandType, argument1, argument2);
    }

    public void SetState(State<TEdge, TArg1, TArg2, TResult> nextState)
    {
        _currentState = nextState;
    }

}

