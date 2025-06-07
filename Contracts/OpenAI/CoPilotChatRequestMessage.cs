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

        public string GetUserFirstAsPlainText()
        {
            var userMessage = Messages?.FirstOrDefault(m => m.Role == ChatMessageRole.User);
            return userMessage.Content;
        }

        public CoPilotChatRequestMessage CreateSingleMessageRequest(string message)
        {
            return new CoPilotChatRequestMessage
            {
                Model = Model,
                Messages = message != null
                    ? new List<CopilotChatMessage> { new CopilotChatMessage { Role = ChatMessageRole.User, Content = message } }
                    : null
            };
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

        public void AddSystemMessage(string content, SystemMessagePriority priority = SystemMessagePriority.Low)
        {
            var systemMessage = new CopilotChatMessage
            {
                Role = ChatMessageRole.System,
                Content = content,
                Priority = priority
            };
            Messages = Messages ?? new List<CopilotChatMessage>();
            Messages.Add(systemMessage);
        }

        public void ReorderMessagesByRoleAndPriority()
        {
            // User messages first (original order), then system messages by priority (lowest to highest)
            var userMessages = Messages.Where(m => m.Role == ChatMessageRole.User).ToList();
            var systemMessages = Messages.Where(m => m.Role == ChatMessageRole.System)
                .OrderBy(m => m.Priority ?? SystemMessagePriority.Low)
                .ToList();
            Messages = userMessages.Concat(systemMessages).ToList();
        }

        public void DeleteAllUserMessages()
        {
            // Remove all user messages from the list
            Messages = Messages?.Where(m => m.Role != ChatMessageRole.User).ToList() ?? new List<CopilotChatMessage>();
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

        public List<string> GetMessagesContent()
        {
            var messagesContent = new List<string>();
            foreach (var message in Messages ?? Enumerable.Empty<CopilotChatMessage>())
            {
                messagesContent.Add(message.Content);
            }

            return messagesContent;
        }
    }
}
