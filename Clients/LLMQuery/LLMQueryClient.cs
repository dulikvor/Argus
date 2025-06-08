using Argus.Common.Data;
using Argus.Contracts.OpenAI;
using Argus.Data;
using OpenAI.Chat;
using System.Collections.Concurrent;

namespace Argus.Clients.LLMQuery
{
    public abstract class LLMQueryClient : ILLMQueryClient
    {
        protected readonly Uri _endpoint;
        protected static readonly ConcurrentDictionary<string, LLMProvider> Clients = new();
        private readonly LLMProviderType _providerType;

        protected LLMQueryClient(Uri endpoint, LLMProviderType providerType)
        {
            _endpoint = endpoint;
            _providerType = providerType;
        }

        protected abstract string GetApiKey();
        protected abstract string GetDefaultModelId();

        protected LLMProvider CreateChatClient()
        {
            var apiKey = GetApiKey();
            return LLMProvider.Create(_providerType, apiKey, _endpoint);
        }

        protected LLMProvider GetOrCreateClient(string userName)
        {
            return Clients.GetOrAdd(userName, (un) => CreateChatClient());
        }

        public async Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            ArgumentValidationHelper.Ensure.NotNull(coPilotChatRequestMessage.Model, "Model");
            var userName = GetUserName();
            var client = GetOrCreateClient(userName);

            var modelId = string.IsNullOrEmpty(coPilotChatRequestMessage.Model) ? GetDefaultModelId() : coPilotChatRequestMessage.Model;
            coPilotChatRequestMessage.ReorderMessagesByRoleAndPriority();
            var completionUpdates = client.CompleteChatStreamingAsync(modelId, coPilotChatRequestMessage.Messages.Select(message => message.Role == ChatMessageRole.User
                        ? (ChatMessage)new UserChatMessage(message.Content)
                        : (ChatMessage)new SystemChatMessage(message.Content)));

            var result = new List<CoPilotChatResponseMessage>();
            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                result.Add(new CoPilotChatResponseMessage(completionUpdate));
            }
            return result;
        }

        public async Task<ChatCompletionStructuredResponse<TResponse>> Query<TResponse>(CoPilotChatRequestMessage coPilotChatRequestMessage, OpenAIStructuredOutput structuredOutput, IList<ChatTool> tools) where TResponse : class
        {
            ArgumentValidationHelper.Ensure.NotNull(coPilotChatRequestMessage.Model, "Model");
            var userName = GetUserName();
            var client = GetOrCreateClient(userName);

            var modelId = string.IsNullOrEmpty(coPilotChatRequestMessage.Model) ? GetDefaultModelId() : coPilotChatRequestMessage.Model;
            
            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions
            {
                ResponseFormat = structuredOutput != null ? ChatResponseFormat.CreateJsonSchemaFormat(structuredOutput.JsonSchemaFormatName, structuredOutput.JsonSchema) : null
            };
            foreach (var tool in tools ?? new List<ChatTool>())
            {
                chatCompletionOptions.Tools.Add(tool);
            }

            coPilotChatRequestMessage.ReorderMessagesByRoleAndPriority();
            var chatCompletion = await client.CompleteChatAsync(
                modelId,
                coPilotChatRequestMessage.Messages.Select(message => message.Role == ChatMessageRole.User
                        ? (ChatMessage)new UserChatMessage(message.Content)
                        : (ChatMessage)new SystemChatMessage(message.Content)),
                chatCompletionOptions);
            return new ChatCompletionStructuredResponse<TResponse>(chatCompletion);
        }

        private string GetUserName()
        {
            return CallContext.GetData(ServiceConstants.Authentication.UserNameKey) as string;
        }
    }
}
