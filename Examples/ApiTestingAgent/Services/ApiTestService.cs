using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Data;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Argus.Data;
using OpenAI.Chat;
using ApiTestsStateContext = Argus.Common.StateMachine.StateContext<ApiTestingAgent.StateMachine.ApiTestStateTransitions, ApiTestingAgent.StateMachine.ApiTestsStepInput, ApiTestingAgent.StateMachine.ApiTestsStepResult>;

namespace ApiTestingAgent.Services
{
    public class ApiTestService : IApiTestService
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
        private readonly IPromptDescriptorFactory _promptDescriptorFactory;
        private readonly IFunctionDescriptorFactory _functionDescriptorFactory;
        private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;

        public ApiTestService(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory, IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
            _promptDescriptorFactory = promptDescriptorFactory;
            _functionDescriptorFactory = functionDescriptorFactory;
            _responseStreamWriter = responseStreamWriter;
        }

        public async Task InvokeNext(HttpContext httpContext, CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var session = SessionStore<ApiTestStateTransitions>.GetSessions((string)CallContext.GetData(ServiceConstants.Authentication.UserNameKey));
            var stateContext = new ApiTestsStateContext(new ServiceInformationState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));

            ApiTestsStepResult result = default;
            ApiTestStateTransitions transition = ApiTestStateTransitions.TestDescriptor;
            do
            {
                var filteredCoPilotChatRequestMessage = coPilotChatRequestMessage.GetUserLast();

                var prompt = _promptDescriptorFactory.GetPromptDescriptor(nameof(ApiTestsPromptDescriptor))
                    .GetPrompt(StatePromptsConstants.ApiTests.Keys.StateMachineKey);
                filteredCoPilotChatRequestMessage.AddSystemMessage(prompt);
                filteredCoPilotChatRequestMessage.AddSystemMessage(session.ToString());

                session.SetCurrentStep(stateContext.GetStateName(), transition);
                (result, transition) = await stateContext.HandleState(session, transition, new ApiTestsStepInput
                {
                    CoPilotChatRequestMessage = filteredCoPilotChatRequestMessage,
                    PreviousStepResult = result
                });

                if(result.CoPilotChatResponseMessages != null)
                {
                    await _responseStreamWriter.WriteToStreamAsync(httpContext, result.CoPilotChatResponseMessages);
                }

                if (stateContext.IsEnd())
                {
                    return;
                }
            }
            while (result.StepSuccess);
        }
    }
}