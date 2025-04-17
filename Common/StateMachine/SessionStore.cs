using System.Collections.Concurrent;

namespace Argus.Common.StateMachine;

public static class SessionStore<TStateTransitions> where TStateTransitions : Enum
{
    static private readonly ConcurrentDictionary<string, Session<TStateTransitions>> Sessions = new ConcurrentDictionary<string, Session<TStateTransitions>>();

    public static Session<TStateTransitions> GetSessions(string user) => Sessions.GetOrAdd(user, new Session<TStateTransitions>());
}

