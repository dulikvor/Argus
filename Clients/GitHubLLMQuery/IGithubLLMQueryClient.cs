using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Clients.GitHubLLMQuery
{
    public interface IGitHubLLMQueryClient
    {
        public Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage);
    }
}
