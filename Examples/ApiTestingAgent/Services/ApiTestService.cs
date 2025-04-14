using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.PromptHandlers;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using ApiTestsStateContext = Argus.Common.StateMachine.StateContext<ApiTestingAgent.StateMachine.ApiTestStateTransitions, ApiTestingAgent.StateMachine.ApiTestsStepInput, ApiTestingAgent.StateMachine.ApiTestsStepResult>;

namespace ApiTestingAgent.Services
{
    public class ApiTestService : IApiTestService
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
        private readonly IPromptHandlerFactory _promptHandlerFactory;
        private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;

        public ApiTestService(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptHandlerFactory promptHandlerFactory, IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
            _promptHandlerFactory = promptHandlerFactory;
            _responseStreamWriter = responseStreamWriter;
        }

        public async Task InvokeNext(HttpContext httpContext, CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var stateContext = new ApiTestsStateContext(new ServiceInformationState(_gitHubLLMQueryClient, _promptHandlerFactory));

            ApiTestsStepResult result = default;
            ApiTestStateTransitions transition = ApiTestStateTransitions.TestDescriptor;
            do
            {
                (result, transition) = await stateContext.HandleState(transition, new ApiTestsStepInput
                {
                    CoPilotChatRequestMessage = coPilotChatRequestMessage
                });
                await _responseStreamWriter.WriteToStreamAsync(httpContext, result.CoPilotChatResponseMessages);

                if(stateContext.IsEnd())
                {
                    return;
                }
            }
            while (result.StepSuccess);
        }
    }
}