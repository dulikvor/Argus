namespace Argus.Common.StateMachine;

using StepResultKey = (string, string);

public class Session<TStateTransitions> where TStateTransitions : Enum
{
    public TStateTransitions CurrentTransition { get; private set; }
    public string CurrentStep { get; private set; }

    Dictionary<StepResultKey, object> StepResult { get; } = new Dictionary<StepResultKey, object>();

    public void AddStepResult(StepResultKey stepResultKey, object value)
    {
        StepResult[stepResultKey] = value;
    }

    public void SetCurrentStep(string step, TStateTransitions transition)
    {
        CurrentStep = step;
        CurrentTransition = transition;
    }

    public override string ToString()
    {
        var stepResults = string.Join(", ", StepResult.Select(kvp => $"[({kvp.Key.Item1}, {kvp.Key.Item2}): {kvp.Value}]"));
        return $"Current Step: {CurrentStep}, Current Transition: {CurrentTransition}, Step Results: {stepResults}";
    }
}

