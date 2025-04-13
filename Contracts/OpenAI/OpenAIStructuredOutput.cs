using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class OpenAIStructuredOutput
    {
        [JsonPropertyName("jsonSchemaFormatName")]
        public string JsonSchemaFormatName { get; }

        [JsonPropertyName("jsonSchema")]
        public BinaryData JsonSchema { get; }

        public OpenAIStructuredOutput(string jsonSchemaFormatName, string jsonSchema)
        {
            JsonSchemaFormatName = jsonSchemaFormatName;
            JsonSchema = BinaryData.FromString(jsonSchema);
        }
    }
}
