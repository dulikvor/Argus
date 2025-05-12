using System.Collections.Concurrent;

namespace Argus.Common.StateMachine;

public static class SessionStore<TSession, TTransition, TStepInput, TStepResult>
    where TSession : Session<TTransition, TStepInput, TStepResult>, new()
    where TTransition : Enum
    where TStepInput : StepInput
    where TStepResult : StepResult
{
    static private readonly ConcurrentDictionary<string, Session<TTransition, TStepInput, TStepResult>> Sessions = new ConcurrentDictionary<string, Session<TTransition, TStepInput, TStepResult>>();

    public static Session<TTransition, TStepInput, TStepResult> GetSessions(string user) => Sessions.GetOrAdd(user, new TSession());
}

