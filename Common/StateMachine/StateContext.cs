namespace Argus.Common.StateMachine;

public class StateContext<TTransition, TStepInput> 
    where TTransition : Enum
    where TStepInput : StepInput
{
    private State<TTransition, TStepInput> _currentState;

    public StateContext(State<TTransition, TStepInput> startingState)
    {
        _currentState = startingState;
    }

    public async Task<(StepResult, TTransition)> HandleState(Session<TTransition, TStepInput> session, TTransition commandType, TStepInput stepInput)
    {
        return await _currentState.HandleState(this, session, commandType, stepInput);
    }

    public void SetState(State<TTransition, TStepInput> nextState)
    {
        _currentState = nextState;
    }

    public State<TTransition, TStepInput> GetCurrentState()
    {
        return _currentState;
    }

    public bool IsEnd()
    {
        return _currentState is EndState<TTransition, TStepInput>;
    }

    public void OnNonSupportedTransition(TTransition transition)
    {
        throw new NotSupportedException($"The transition '{transition}' is not supported in the current state.");
    }

}

