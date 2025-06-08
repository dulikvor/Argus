namespace Argus.Common.Builtin.PromptDescriptor
{
    public static class PromptsConstants
    {
        public static class Prompts
        {
            public static class Keys
            {
                public const string EndState = $"{nameof(EndState)}";
                public const string Context = $"{nameof(Context)}";
                public const string CurrentState = $"{nameof(CurrentState)}";
                public const string CustomerConsentStateTransition = $"CustomerConsentStateTransition";
            }
        }

        public static class StructuredResponses
        {
            public static class Keys
            {
                public const string CustomerConsentStateTransitionResponseSchema = "CustomerConsentStateTransitionResponseSchema";
                public const string StringResponseSchema = "StringResponseSchema";
            }
        }
    }
}
