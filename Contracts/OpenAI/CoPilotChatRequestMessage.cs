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
                Messages = Messages?.Select(m => m.Clone()).ToList()
            };
        }

        public CoPilotChatRequestMessage GetUserLast()
        {
            var userMessage = Messages?.LastOrDefault(m => m.Role == ChatMessageRole.User);
            return CreateSingleMessageRequest(userMessage);
        }

        public CoPilotChatRequestMessage GetUserFirst()
        {
            var userMessage = Messages?.FirstOrDefault(m => m.Role == ChatMessageRole.User);
            return CreateSingleMessageRequest(userMessage);
        }

        private CoPilotChatRequestMessage CreateSingleMessageRequest(CopilotChatMessage message)
        {
            return new CoPilotChatRequestMessage
            {
                Model = Model,
                Messages = message != null
                    ? new List<CopilotChatMessage> { message.Clone() }
                    : null
            };
        }

        public ConfirmationState? GetConfirmation(string confirmationId)
        {
            // Iterate through all messages to find the confirmation with the given ID
            foreach (var message in Messages ?? Enumerable.Empty<CopilotChatMessage>())
            {
                var confirmation = message.CopilotConfirmations?.FirstOrDefault(c => c.Confirmation.Id == confirmationId);
                if (confirmation != null)
                {
                    return confirmation.State;
                }
            }

            // Return null if no matching confirmation is found
            return null;
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

        public void AddUserMessage(string content)
        {
            var userMessage = new CopilotChatMessage
            {
                Role = ChatMessageRole.User,
                Content = content
            };
            Messages = Messages ?? new List<CopilotChatMessage>();
            Messages.Add(userMessage);
        }
    }
}
