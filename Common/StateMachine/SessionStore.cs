using System.Collections.Concurrent;

namespace Argus.Common.StateMachine;

public static class SessionStore<TSession, TTransition, TStepInput, TStepResult>
    where TSession : Session<TTransition, TStepInput>, new()
    where TTransition : Enum
    where TStepInput : StepInput
    where TStepResult : StepResult
{
    static private readonly ConcurrentDictionary<string, Session<TTransition, TStepInput>> Sessions = new ConcurrentDictionary<string, Session<TTransition, TStepInput>>();

    public static Session<TTransition, TStepInput> GetSessions(string user) => Sessions.GetOrAdd(user, new TSession());
}

