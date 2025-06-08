using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Argus.Clients.LLMQuery
{
    public abstract class LLMProvider
    {
        public static LLMProvider Create(
            LLMProviderType providerType,
            string apiKey,
            Uri endpoint)
        {
            switch (providerType)
            {
                case LLMProviderType.Azure:
                    return new AzureLLMProvider(endpoint, apiKey);
                case LLMProviderType.OpenAI:
                    // Implement OpenAIProvider if needed
                    return new OpenAILLMProvider(endpoint, apiKey);
                default:
                    throw new ArgumentOutOfRangeException(nameof(providerType), providerType, null);
            }
        }

        public abstract Task<ChatCompletion> CompleteChatAsync(string modelId, IEnumerable<ChatMessage> messages, ChatCompletionOptions options = null, CancellationToken cancellationToken = default);
        public abstract AsyncCollectionResult<StreamingChatCompletionUpdate> CompleteChatStreamingAsync(string modelId, IEnumerable<ChatMessage> messages, ChatCompletionOptions options = null, CancellationToken cancellationToken = default);
    }
}
