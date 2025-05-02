namespace ApiTestingAgent.StateMachine
{
    public enum ApiTestStateTransitions
    {
        ServiceInformationDiscovery,
        RestDiscovery,
        RawContentGet,
        RestCompile,
        CommandDiscovery,
        ExpectedOutcome,
        CommandInvocation,
        Any
    }
}
