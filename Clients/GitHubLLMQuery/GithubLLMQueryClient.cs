using Argus.Common.Data;
using Argus.Contracts.OpenAI;
using Argus.Data;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Concurrent;

namespace Argus.Clients.GitHubLLMQuery
{
    public class GitHubLLMQueryClient : IGitHubLLMQueryClient
    {
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
            var chatClient = client.GetChatClient(string.IsNullOrEmpty(coPilotChatRequestMessage.Model) ? "gpt-4o" : coPilotChatRequestMessage.Model);
            
            var completionUpdates = chatClient.CompleteChatStreamingAsync(
                coPilotChatRequestMessage.Messages.Select(message => new UserChatMessage(message.Content)));

            var result = new List<CoPilotChatResponseMessage>();
            await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                result.Add(new CoPilotChatResponseMessage(completionUpdate));
            }
            return result;
        }

        private OpenAIClient GetOrCreateClient(string userName)
        {
            var client = Clients.GetOrAdd(userName, (un) => CreateChatClient());
            return client;
        }
    }
}
