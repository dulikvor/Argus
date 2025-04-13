using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.PromptHandlers;
using Argus.Contracts.OpenAI;
using ApiTestsStateContext = Argus.Common.StateMachine.StateContext<ApiTestingAgent.StateMachine.ApiTestStateTransitions, ApiTestingAgent.StateMachine.ApiTestsStepInput, ApiTestingAgent.StateMachine.ApiTestsStepResult>;

namespace ApiTestingAgent.Services
{
    public class ApiTestService : IApiTestService
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
        private readonly IPromptHandlerFactory _promptHandlerFactory;

        public ApiTestService(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptHandlerFactory promptHandlerFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
            _promptHandlerFactory = promptHandlerFactory;
        }

        public async Task<IReadOnlyList<CoPilotChatResponseMessage>> InvokeNext(CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var stateContext = new ApiTestsStateContext(new ServiceInformationState(_gitHubLLMQueryClient, _promptHandlerFactory));
            var result = await stateContext.HandleState(ApiTestStateTransitions.TestDescriptor, new ApiTestsStepInput
            {
                CoPilotChatRequestMessage = coPilotChatRequestMessage
            });

            return result.CoPilotChatResponseMessages;
        }
    }
}