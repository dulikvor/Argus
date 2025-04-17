using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;

namespace Argus.Common.StateMachine;

public abstract class State<TTransition, TStepInput, TStepResult> where TTransition : Enum
{
    protected readonly IPromptDescriptorFactory _promptDescriptorFactory;
    protected readonly IFunctionDescriptorFactory _functionDescriptorFactory;

    protected State(IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory)
    {
        _promptDescriptorFactory = promptDescriptorFactory;
        _functionDescriptorFactory = functionDescriptorFactory;
    }

    public virtual string GetName() => throw new InvalidOperationException();

    public virtual Task<(TStepResult, TTransition)> HandleState(StateContext<TTransition, TStepInput, TStepResult> context, Session<TTransition> session, TTransition command, TStepInput stepInput)
    {
        throw new InvalidOperationException();
    }
}