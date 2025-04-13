using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Clients.GitHubLLMQuery
{
    public interface IGitHubLLMQueryClient
    {
        public Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage);
        public Task<(TResponse, ChatCompletion)> Query<TResponse>(CoPilotChatRequestMessage coPilotChatRequestMessage, OpenAIStructuredOutput structuredOutput) where TResponse : class;

    }
}
