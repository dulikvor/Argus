using Argus.Common.Data;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using static Azure.AI.OpenAI.AzureOpenAIClientOptions;

namespace Argus.Clients.LLMQuery
{
    public class AzureLLMProvider : LLMProvider
    {
        private readonly AzureOpenAIClient _client;

        public AzureLLMProvider(Uri endpoint, string apiKey)
        {
            _client = new AzureOpenAIClient(endpoint, new Azure.AzureKeyCredential(apiKey), new AzureOpenAIClientOptions(ServiceVersion.V2024_10_21));
        }

        public override async Task<ChatCompletion> CompleteChatAsync(string modelId, IEnumerable<ChatMessage> messages, ChatCompletionOptions options = null, CancellationToken cancellationToken = default)
        {
            ArgumentValidationHelper.Ensure.NotNull(modelId, "ModelId");
            var chatClient = _client.GetChatClient(modelId);
            return await chatClient.CompleteChatAsync(messages, options);
        }

        public override AsyncCollectionResult<StreamingChatCompletionUpdate> CompleteChatStreamingAsync(string modelId, IEnumerable<ChatMessage> messages, ChatCompletionOptions options = null, CancellationToken cancellationToken = default)
        {
            ArgumentValidationHelper.Ensure.NotNull(modelId, "ModelId");
            var chatClient = _client.GetChatClient(modelId);
            return chatClient.CompleteChatStreamingAsync(messages, options);
        }
    }
}
