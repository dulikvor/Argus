namespace ApiTestingAgent.StateMachine
{
    public enum ApiTestStateTransitions
    {
        ServiceInformationDiscovery,
        RestDiscovery,
        RawContentGet,
        RestCompile,
        CommandDiscovery,
        CommandConsent,
        CommandConsentApproval,
        Any
    }
}
