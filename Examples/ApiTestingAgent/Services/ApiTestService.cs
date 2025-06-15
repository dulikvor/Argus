using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Clients.LLMQuery;
using Argus.Common.Data;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Argus.Data;
using System.Diagnostics;
using ApiTestsStateContext = Argus.Common.StateMachine.StateContext<ApiTestingAgent.StateMachine.ApiTestStateTransitions, Argus.Common.StateMachine.StepInput>;

namespace ApiTestingAgent.Services
{
    public class ApiTestService : IApiTestService
    {
        private readonly IAzureLLMQueryClient _llmQueryClient;
        private readonly IPromptDescriptorFactory _promptDescriptorFactory;
        private readonly IFunctionDescriptorFactory _functionDescriptorFactory;
        private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;
        private readonly ISemanticStore _semanticStore;
        private readonly ILogger<State<ApiTestStateTransitions, StepInput>> _logger;
        private readonly StreamReporter _streamReporter;
        private readonly IStateFactory _stateFactory;

        public ApiTestService(
            IServiceProvider serviceProvider,
            IAzureLLMQueryClient llmQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory, 
            IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter,
            ISemanticStore semanticStore,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger,
            StreamReporter streamReporter,
            IStateFactory stateFactory)
        {
            _llmQueryClient = llmQueryClient;
            _promptDescriptorFactory = promptDescriptorFactory;
            _functionDescriptorFactory = functionDescriptorFactory;
            _responseStreamWriter = responseStreamWriter;
            _semanticStore = semanticStore;
            _logger = logger;
            _streamReporter = streamReporter;
            _stateFactory = stateFactory;
        }

        public async Task InvokeNext(HttpContext httpContext, CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            using var activityScope = ActivityScope.Create(nameof(ApiTestService));
            await activityScope.Monitor(async () =>
            {
                activityScope.Activity.SetTag("user", CallContext.GetData(ServiceConstants.Authentication.UserNameKey)?.ToString() ?? "unknown");
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
                    stateContext = new ApiTestsStateContext(_stateFactory.Create<DomainSelectionState, ApiTestStateTransitions, StepInput>());
                    transition = ApiTestStateTransitions.ServiceInformationDiscovery;
                }

                StepResult result = default;
                do
                {
                    var filteredCoPilotChatRequestMessage = string.IsNullOrEmpty(result?.OverrideUserMessage)
                        ? coPilotChatRequestMessage.GetUserLast()
                        : coPilotChatRequestMessage.CreateSingleMessageRequest(result.OverrideUserMessage);

                    session.SetCurrentStep(stateContext.GetCurrentState(), transition);
                    filteredCoPilotChatRequestMessage.AddSystemMessage(session.ToString(), SystemMessagePriority.Medium);

                    (result, transition) = await stateContext.HandleState(session, transition, new StepInput
                    {
                        CoPilotChatRequestMessage = filteredCoPilotChatRequestMessage,
                        PreviousStepResult = result
                    });

                    if (result.CoPilotChatResponseMessages != null)
                    {
                        await _streamReporter.ReportAsync(result.CoPilotChatResponseMessages, httpContext);
                    }

                    if (result.ConfirmationMessage != null)
                    {
                        await _streamReporter.ReportAsync(result.ConfirmationMessage, httpContext);
                    }
                }
                while (result.StepSuccess);
            });
        }
    }
}