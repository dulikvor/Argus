using Argus.Common.Data;
using Argus.Contracts.OpenAI;
using Argus.Data;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Argus.Clients.GitHubLLMQuery
{
    public class GitHubLLMQueryClient : IGitHubLLMQueryClient
    {
        private const string DefaultLLMModel = "gpt-4o";
        private readonly GitHubLLMQueryClientOptions _clientOptions;
        private static readonly ConcurrentDictionary<string, OpenAIClient> Clients;

        static GitHubLLMQueryClient()
        {
            Clients = new ConcurrentDictionary<string, OpenAIClient>();
        }

        public GitHubLLMQueryClient(IOptions<GitHubLLMQueryClientOptions> clientOptionsAccessor)
        {
            _clientOptions = clientOptionsAccessor.Value;
        }

        public OpenAIClient CreateChatClient()
        {
            var token = CallContext.GetData(ServiceConstants.Authentication.GitHubTokenKey) as string;
            return new OpenAIClient(new ApiKeyCredential(token), new OpenAIClientOptions()
            {
                Endpoint = _clientOptions.Endpoint,
                UserAgentApplicationId = ServiceConstants.ServiceApplicationId
            });
        }

        public async Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            ArgumentValidationHelper.Ensure.NotNull(coPilotChatRequestMessage.Model, "Model");

            var userName = CallContext.GetData(ServiceConstants.Authentication.UserNameKey) as string;
            var client = GetOrCreateClient(userName);
            var chatClient = client.GetChatClient(string.IsNullOrEmpty(coPilotChatRequestMessage.Model) ? DefaultLLMModel : coPilotChatRequestMessage.Model);

            var completionUpdates = chatClient.CompleteChatStreamingAsync(
                coPilotChatRequestMessage.Messages.Select(message => new UserChatMessage(message.Content)));

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
            ArgumentValidationHelper.Ensure.NotNull(structuredOutput, "StructuredOutput");

            var userName = CallContext.GetData(ServiceConstants.Authentication.UserNameKey) as string;
            var client = GetOrCreateClient(userName);
            var chatClient = client.GetChatClient(string.IsNullOrEmpty(coPilotChatRequestMessage.Model) ? DefaultLLMModel : coPilotChatRequestMessage.Model);

            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(structuredOutput.JsonSchemaFormatName, structuredOutput.JsonSchema),

            };

            foreach (var tool in tools ?? new List<ChatTool>())
            {
                chatCompletionOptions.Tools.Add(tool);
            }

            var chatCompletion = await chatClient.CompleteChatAsync(
                    coPilotChatRequestMessage.Messages.Select(message => message.Role == ChatMessageRole.User 
                        ? (ChatMessage)new UserChatMessage(message.Content) 
                        : (ChatMessage)new SystemChatMessage(message.Content)), chatCompletionOptions);

            return new ChatCompletionStructuredResponse<TResponse>(chatCompletion.Value);
        }

        private OpenAIClient GetOrCreateClient(string userName)
        {
            var client = Clients.GetOrAdd(userName, (un) => CreateChatClient());
            return client;
        }
    }
}
