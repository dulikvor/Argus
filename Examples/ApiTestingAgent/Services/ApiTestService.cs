using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Data;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Argus.Data;
using ApiTestsStateContext = Argus.Common.StateMachine.StateContext<ApiTestingAgent.StateMachine.ApiTestStateTransitions, Argus.Common.StateMachine.StepInput>;

namespace ApiTestingAgent.Services
{
    public class ApiTestService : IApiTestService
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
        private readonly IPromptDescriptorFactory _promptDescriptorFactory;
        private readonly IFunctionDescriptorFactory _functionDescriptorFactory;
        private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;
        private readonly ISemanticStore _semanticStore;

        public ApiTestService(
            IServiceProvider serviceProvider,
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory, 
            IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter,
            ISemanticStore semanticStore)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
            _promptDescriptorFactory = promptDescriptorFactory;
            _functionDescriptorFactory = functionDescriptorFactory;
            _responseStreamWriter = responseStreamWriter;
            _semanticStore = semanticStore;
        }

        public async Task InvokeNext(HttpContext httpContext, CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var session = SessionStore<ApiTestSession, ApiTestStateTransitions, StepInput, StepResult>.GetSessions((string)CallContext.GetData(ServiceConstants.Authentication.UserNameKey));

            ApiTestStateTransitions transition = default;
            ApiTestsStateContext stateContext = default;
            if (session.CurrentStep != null)
            { 
                stateContext = new ApiTestsStateContext(session.CurrentStep);
                transition = session.CurrentTransition;
            }
            else
            {
                stateContext = new ApiTestsStateContext(new ServiceInformationState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore));
                transition = ApiTestStateTransitions.ServiceInformationDiscovery;
            }

            StepResult result = default;
            do
            {
                var filteredCoPilotChatRequestMessage = coPilotChatRequestMessage.GetUserLast();

                var prompt = _promptDescriptorFactory.GetPromptDescriptor(nameof(ApiTestsPromptDescriptor))
                    .GetPrompt(PromptsConstants.ApiTests.Keys.StateMachineKey);
                filteredCoPilotChatRequestMessage.AddSystemMessage(prompt);

                session.SetCurrentStep(stateContext.GetCurrentState(), transition);
                filteredCoPilotChatRequestMessage.AddSystemMessage(session.ToString());

                (result, transition) = await stateContext.HandleState(session, transition, new StepInput
                {
                    CoPilotChatRequestMessage = filteredCoPilotChatRequestMessage,
                    PreviousStepResult = result
                });

                if(result.CoPilotChatResponseMessages != null)
                {
                    await _responseStreamWriter.WriteToStreamAsync(httpContext, result.CoPilotChatResponseMessages);
                }

                if (result.ConfirmationMessage != null)
                {
                    await _responseStreamWriter.WriteToStreamAsync(httpContext, new List<object> { result.ConfirmationMessage }, EventType.CopilotConfirmation);
                }
            }
            while (result.StepSuccess);
        }
    }
}