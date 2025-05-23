﻿namespace Argus.Common.StateMachine;

using StepResultKey = (string, string);

public class Session<TTransition, TStepInput, TStepResult>
    where TTransition : Enum
    where TStepInput : StepInput
    where TStepResult : StepResult
{
    public TTransition CurrentTransition { get; private set; }
    public State<TTransition, TStepInput, TStepResult> CurrentStep { get; private set; }
    public string CurrentConfirmationId { get; private set; } // Tracks the current confirmation ID.

    protected Dictionary<StepResultKey, object> StepResult { get; } = new Dictionary<StepResultKey, object>();

    public void AddStepResult(StepResultKey stepResultKey, object value)
    {
        StepResult[stepResultKey] = value;
    }

    public void SetCurrentStep(State<TTransition, TStepInput, TStepResult> step, TTransition transition)
    {
        CurrentStep = step;
        CurrentTransition = transition;
    }

    public void SetCurrentConfirmationId(string confirmationId)
    {
        CurrentConfirmationId = confirmationId;
    }

    public void ResetConfirmationId()
    {
        CurrentConfirmationId = null;
    }

    public override string ToString()
    {
        var stepResults = string.Join(", ", StepResult.Select(kvp => $"[({kvp.Key.Item1.ToString()}, {kvp.Key.Item2.ToString()}): {kvp.Value.ToString()}]"));
        return $"Current Step: {CurrentStep.GetName()}, Current Transition: {CurrentTransition.ToString()}, Step Results: {stepResults}";
    }

    public static StepResultKey CompileKey(string stepName, string key)
    {
        return (stepName, key);
    }
}

