namespace ApiTestingAgent.StateMachine
{
    public static class StatePromptsConstants
    {
        public static class ServiceInformation
        {
            public static class Keys
            {
                public const string ServiceInformationDomainPromptKey = $"{nameof(ServiceInformationDomainPromptKey)}";
                public const string ServiceInformationDomainReturnedOutputKey = $"{nameof(ServiceInformationDomainReturnedOutputKey)}";
            }
        }

        public static class RestDiscovery
        {
            public static class Keys
            {
                public const string RestResourcesDiscoveryPromptKey = $"{nameof(RestResourcesDiscoveryPromptKey)}";
                public const string RestResourcesDiscoveryReturnedOutputKey = $"{nameof(RestResourcesDiscoveryReturnedOutputKey)}";
            }
        }
    }
}
