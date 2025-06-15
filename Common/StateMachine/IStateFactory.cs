namespace Argus.Common.StateMachine
{
    public interface IStateFactory
    {
        State<TTransition, TStepInput> Create<TState, TTransition, TStepInput>()
            where TState : State<TTransition, TStepInput>
            where TTransition : System.Enum
            where TStepInput : StepInput;

        State<TTransition, TStepInput> Create<TTransition, TStepInput>(System.Type stateType)
            where TTransition : System.Enum
            where TStepInput : StepInput;
    }
}
