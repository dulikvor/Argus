using OpenAI.Chat;
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
        public List<CopilotChatMessage> Messages { get; set; }

        public CoPilotChatRequestMessage Clone()
        {
            return new CoPilotChatRequestMessage
            {
                Model = Model,
                Messages = Messages?.Select(m => new CopilotChatMessage
                {
                    Role = m.Role,
                    Content = m.Content
                }).ToList()
            };
        }

        public void AddSystemMessage(string content)
        {
            var systemMessage = new CopilotChatMessage
            {
                Role = ChatMessageRole.System,
                Content = content
            };
            Messages = Messages ?? new List<CopilotChatMessage>();
            Messages.Add(systemMessage);
        }
    }
}
