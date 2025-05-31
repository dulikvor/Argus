using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class EmbeddingRequest
    {
        [JsonPropertyName("input")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Input { get; set; }
    }
}