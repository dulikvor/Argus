using Argus.Common.Data;
using Argus.Data;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Argus.Clients.LLMQuery
{
    public class OpenAILLMProvider : LLMProvider
    {
        private readonly OpenAIClient _client;

        public OpenAILLMProvider(Uri endpoint, string apiKey)
        {
            _client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions()
            {
                Endpoint = endpoint,
                UserAgentApplicationId = ServiceConstants.ServiceApplicationId
            });
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
