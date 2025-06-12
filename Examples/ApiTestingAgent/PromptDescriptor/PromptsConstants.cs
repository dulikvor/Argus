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
                public const string SessionResultFunctionSourceArgumentsFormat = "Tool {0} last known request arguments as detected by LLM";
                public const string SessionResultFunctionFormat = "Tool {0} last known result";
            }
        }

        public static class ApiTests
        {
            public static class Keys
            {
                public const string StateMachineKey = $"{nameof(StateMachineKey)}";
                public const string DetectedOperationsKey = $"{nameof(DetectedOperationsKey)}";
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
                public const string PostRunSwaggerSummaryPromptKey = $"{nameof(PostRunSwaggerSummaryPromptKey)}";
                public const string RestResourcesDiscoveryReturnedOutputKey = $"{nameof(RestResourcesDiscoveryReturnedOutputKey)}";
            }
        }

        public static class CommandSelect
        {
            public static class Keys
            {
                public const string SelectedCommandKey = $"{nameof(SelectedCommandKey)}";
                public const string RestSelectPromptKey = $"{nameof(RestSelectPromptKey)}";
                public const string RestSelectReturnedOutputKey = $"{nameof(RestSelectReturnedOutputKey)}";
            }
        }

        public static class ExpectedOutcome
        {
            public static class Keys
            {
                public const string ExpectedOutcomePromptKey = "ExpectedOutcomePromptKey";
                public const string ExpectedOutcomeReturnedOutputKey = "ExpectedOutcomeReturnedOutputKey";
            }
        }

        public static class CommandInvocation
        {
            public static class Keys
            {
                public const string HttpMethod = "HttpMethod";
                public const string ResponseContent = "ResponseContent";
                public const string CommandInvocationPromptKey = "CommandInvocationPromptKey";
                public const string CommandInvocationAnalysisPromptKey = "CommandInvocationAnalysisPromptKey";
                public const string CommandInvocationAnalysisReturnedOutputKey = "CommandInvocationAnalysisReturnedOutputKey";
                public const string CommandInvocationHttpResultExplanationPromptKey = "CommandInvocationHttpResultExplanationPromptKey";
                public const string CommandInvocationDetectNextStatePromptKey = "CommandInvocationDetectNextStatePromptKey";
                public const string CommandInvocationDetectNextStateOutputKey = "CommandInvocationDetectNextStateOutputKey";
            }
        }
    }
}
