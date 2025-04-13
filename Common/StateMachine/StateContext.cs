namespace Argus.Common.StateMachine;

public class StateContext<TTransition, TStepInput, TStepResult> where TTransition : Enum
{
    private State<TTransition, TStepInput, TStepResult> _currentState;

    public StateContext(State<TTransition, TStepInput, TStepResult> startingState)
    {
        _currentState = startingState;
    }

    public async Task<TStepResult> HandleState(TTransition commandType, TStepInput stepInput)
    {
        return await _currentState.HandleState(this, commandType, stepInput);
    }

    public void SetState(State<TTransition, TStepInput, TStepResult> nextState)
    {
        _currentState = nextState;
    }

    public void OnNonSupportedTransition(TTransition transition)
    {
        throw new NotSupportedException($"The transition '{transition}' is not supported in the current state.");
    }

}

