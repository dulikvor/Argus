using Argus.Common.Functions;

namespace ApiTestingAgent.PromptDescriptor
{
    public static class StatePromptsConstants
    {
        public static class SessionResult
        {
            public const string SessionResultFunctionFormat = "Tool {0} result)";
        }

        public static class ApiTests
        {
            public static class Keys
            {
                public const string StateMachineKey = $"{nameof(StateMachineKey)}";
            }
        }

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
