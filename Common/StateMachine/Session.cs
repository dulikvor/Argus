﻿namespace Argus.Common.StateMachine;

using StepResultKey = (string, string);

public class Session<TTransition, TStepInput>
    where TTransition : Enum
    where TStepInput : StepInput
{
    public const string IncrementalResultKeyPostfix = "IncrementalResult";

    public TTransition CurrentTransition { get; private set; }
    public State<TTransition, TStepInput> CurrentStep { get; private set; }
    public string CurrentConfirmationId { get; private set; } // Tracks the current confirmation ID.

    public Dictionary<StepResultKey, object> StepResult { get; } = new Dictionary<StepResultKey, object>();

    public void AddStepResult(StepResultKey stepResultKey, object value)
    {
        StepResult[stepResultKey] = value;
    }

    public void ResetStepResult(StepResultKey stepResultKey)
    {
        if (StepResult.ContainsKey(stepResultKey))
        {
            StepResult.Remove(stepResultKey);
        }
    }

    public void SetCurrentStep(State<TTransition, TStepInput> step, TTransition transition)
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

    public bool HasPendingConfirmation()
    {
        return !string.IsNullOrEmpty(CurrentConfirmationId);
    }

    public override string ToString()
    {
        var stepResults = string.Join('\n', StepResult.Select(kvp => kvp.Value.ToString()));
        return $"*As Context for you*\n\nSelections made by the user in previous steps:\n{stepResults}";
    }

    public static StepResultKey CompileKey(string stepName, string key)
    {
        return (stepName, key);
    }
}

