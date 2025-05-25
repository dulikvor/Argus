using OpenAI.Chat;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI
{
    public class CoPilotChatChoice
    {
        public class MessageContent
        {
            [JsonPropertyName("type")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ChatMessageContentPartKind Type { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        public class CoPilotChatChoiceDelta
        {
            [JsonPropertyName("role")]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public ChatMessageRole? Role { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        public CoPilotChatChoice(StreamingChatCompletionUpdate streamingChatCompletionUpdate)
        {
            FinishReason = streamingChatCompletionUpdate.FinishReason?.ToString().ToLower();
            Index = 0;
            Delta = new CoPilotChatChoiceDelta()
            {
                Role = streamingChatCompletionUpdate.Role,
                Content = streamingChatCompletionUpdate.ContentUpdate.Select(cu => cu.Text).FirstOrDefault()
            };

        }

        public CoPilotChatChoice(string content, bool isUnconcluded)
        {
            FinishReason = isUnconcluded ? null : ChatFinishReason.Stop.ToString().ToLower();
            Index = 0;
            Delta = new CoPilotChatChoiceDelta()
            {
                Role = ChatMessageRole.Assistant,
                Content = content
            };
        }

        [JsonPropertyName("index")]
        public int Index { get; }

        [JsonPropertyName("delta")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public CoPilotChatChoiceDelta Delta { get; }

        [JsonPropertyName("finish_reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string FinishReason { get; }

        [JsonPropertyName("contentTokenLogProbabilities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IReadOnlyList<ChatTokenLogProbabilityDetails> ContentTokenLogProbabilities { get; }

        [JsonPropertyName("refusalTokenLogProbabilities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IReadOnlyList<ChatTokenLogProbabilityDetails> RefusalTokenLogProbabilities { get; }
    }

    public record UsageResponse
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

    }

    public class CoPilotChatResponseMessage
    {
        public CoPilotChatResponseMessage(StreamingChatCompletionUpdate streamingChatCompletionUpdate)
        {
            Choices = new List<CoPilotChatChoice> { new CoPilotChatChoice(streamingChatCompletionUpdate) };
            Model = streamingChatCompletionUpdate.Model;
            Id = streamingChatCompletionUpdate.CompletionId;
            SystemFingerprint = streamingChatCompletionUpdate.SystemFingerprint;
            Usage = streamingChatCompletionUpdate.Usage != null
                ? new UsageResponse
                {
                    PromptTokens = streamingChatCompletionUpdate.Usage.InputTokenCount,
                    CompletionTokens = streamingChatCompletionUpdate.Usage.OutputTokenCount,
                    TotalTokens = streamingChatCompletionUpdate.Usage.TotalTokenCount
                }
                : null;
        }

        public CoPilotChatResponseMessage(string content, ChatCompletion chatCompletion, bool isUnconcluded)
        {
            Choices = new List<CoPilotChatChoice> { new CoPilotChatChoice(content, isUnconcluded) };
            Model = chatCompletion?.Model;
            Id = chatCompletion?.Id;
            SystemFingerprint = chatCompletion?.SystemFingerprint;
            Usage = isUnconcluded == false && chatCompletion?.Usage != null
                ? new UsageResponse
                {
                    PromptTokens = chatCompletion.Usage.InputTokenCount,
                    CompletionTokens = chatCompletion.Usage.OutputTokenCount,
                    TotalTokens = chatCompletion.Usage.TotalTokenCount
                }
                : null;
            CreatedAt = chatCompletion?.CreatedAt.Ticks ?? default;
        }

        public CoPilotChatResponseMessage() { }

        [JsonPropertyName("model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Model { get; set; }

        [JsonPropertyName("usage")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public UsageResponse Usage { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("system_fingerprint")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string SystemFingerprint { get; set; }

        [JsonPropertyName("created")]
        public long CreatedAt { get; }

        [JsonPropertyName("choices")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public IReadOnlyList<CoPilotChatChoice> Choices { get; }
    }
}
