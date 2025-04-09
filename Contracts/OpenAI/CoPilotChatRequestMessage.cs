using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class CoPilotChatRequestMessage
    {
        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public CopilotChatMessage[] Messages { get; set; }
    }
}
