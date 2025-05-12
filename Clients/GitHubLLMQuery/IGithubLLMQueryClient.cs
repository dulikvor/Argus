using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Clients.GitHubLLMQuery
{
    public interface IGitHubLLMQueryClient
    {
        public Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage);
        public Task<ChatCompletionStructuredResponse<TResponse>> Query<TResponse>(CoPilotChatRequestMessage coPilotChatRequestMessage, OpenAIStructuredOutput structuredOutput, IList<ChatTool> tools, bool toolsRequired = false) where TResponse : class;

    }
}
