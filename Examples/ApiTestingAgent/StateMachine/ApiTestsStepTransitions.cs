namespace ApiTestingAgent.StateMachine
{
    public enum ApiTestStateTransitions
    {
        TestDescriptor,
        RestDiscovery,
        RawContentGet,
        RestCompile,
        CommandDiscovery,
        CommandConsent,
        CommandConsentApproval,
        Any
    }
}
