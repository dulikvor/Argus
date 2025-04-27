using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class CopilotChatMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("role")]
        public ChatMessageRole Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("copilot_confirmations")]
        public List<CopilotConfirmationResponseMessage> CopilotConfirmations { get; set; } // List of copilot confirmations.
    }
}