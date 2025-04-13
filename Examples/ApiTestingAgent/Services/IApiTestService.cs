using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.Services
{
    public interface IApiTestService
    {
        public Task<IReadOnlyList<CoPilotChatResponseMessage>> InvokeNext(CoPilotChatRequestMessage coPilotChatRequestMessage);
    }
}
