namespace ApiTestingAgent.StateMachine
{
    public enum ApiTestStateTransitions
    {
        ServiceInformationDiscovery,
        RestDiscovery,
        RawContentGet,
        RestCompile,
        CommandSelect,
        ExpectedOutcome,
        CommandInvocation,
        CommandInvocationAnalysis,
        Any
    }
}
