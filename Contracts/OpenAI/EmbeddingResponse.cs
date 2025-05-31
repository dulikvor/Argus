using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class Embedding
    {
        [JsonPropertyName("embedding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public float[] EmbeddingVector { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class EmbeddingResponse
    {
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Model { get; set; }

        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<Embedding> Data { get; set; }

        [JsonPropertyName("usage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public UsageResponse Usage { get; set; }
    }
}
