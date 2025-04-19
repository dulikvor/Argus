namespace Argus.Common.StateMachine;

public class StateContext<TTransition, TStepInput, TStepResult> 
    where TTransition : Enum
    where TStepInput : StepInput
    where TStepResult : StepResult
{
    private State<TTransition, TStepInput, TStepResult> _currentState;

    public StateContext(State<TTransition, TStepInput, TStepResult> startingState)
    {
        _currentState = startingState;
    }

    public async Task<(TStepResult, TTransition)> HandleState(Session<TTransition, TStepInput, TStepResult> session, TTransition commandType, TStepInput stepInput)
    {
        return await _currentState.HandleState(this, session, commandType, stepInput);
    }

    public void SetState(State<TTransition, TStepInput, TStepResult> nextState)
    {
        _currentState = nextState;
    }

    public State<TTransition, TStepInput, TStepResult> GetCurrentState()
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

