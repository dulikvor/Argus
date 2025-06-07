using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace Argus.Common.Builtin.PromptDescriptor
{
    public class StringPromptDescriptor : BasePromptDescriptor
    {
        public override string DescriptorType => nameof(StringPromptDescriptor);

        public StringPromptDescriptor()
        {
            this.Initialize();
        }

        protected override void Initialize()
        {
            // Initialize structured responses
            var stringResponseSchema = new
            {
                type = "object",
                properties = new
                {
                    content = new { type = "string", description = "A generic string response returned from the model. The meaning depends on the specific use case." }
                },
                required = new[] { "content" }
            };

            StructuredResponses.Add<StringResponse>(PromptsConstants.StructuredResponses.Keys.StringResponseSchema,
                JsonSerializer.Serialize(stringResponseSchema));
        }
    }
}
