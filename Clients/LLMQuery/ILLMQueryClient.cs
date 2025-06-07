using Argus.Common.Data;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Clients.LLMQuery
{
    public interface ILLMQueryClient
    {
        Task<IReadOnlyList<CoPilotChatResponseMessage>> Query(CoPilotChatRequestMessage coPilotChatRequestMessage);
        Task<ChatCompletionStructuredResponse<TResponse>> Query<TResponse>(CoPilotChatRequestMessage coPilotChatRequestMessage, OpenAIStructuredOutput structuredOutput, IList<ChatTool> tools) where TResponse : class;
    }
}
