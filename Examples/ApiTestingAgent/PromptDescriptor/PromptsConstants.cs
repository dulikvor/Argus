using Argus.Common.Functions;

namespace ApiTestingAgent.PromptDescriptor
{
    public static class PromptsConstants
    {
        public static class SessionResult
        {
            public static class Keys
            {
                public const string SessionResultSessionDomain = "Found Domain";
                public const string DetectedResourcesKey = "DetectedResources";
            }

            public static class Formats
            {
                public const string SessionResultFunctionFormat = "Tool {0} result";
            }
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

        public static class CommandDiscovery
        {
            public static class Keys
            {
                public const string RestSelectPromptKey = $"{nameof(RestSelectPromptKey)}";
                public const string RestSelectReturnedOutputKey = $"{nameof(RestSelectReturnedOutputKey)}";
            }
        }
    }
}
