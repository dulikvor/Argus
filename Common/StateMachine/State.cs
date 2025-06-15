using Argus.Clients.LLMQuery;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using Argus.Common.Web;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using Argus.Common.Orchestration;

using StepResultKey = (string, string);

namespace Argus.Common.StateMachine;

public abstract class State<TTransition, TStepInput>
    where TTransition : Enum
    where TStepInput : StepInput
{
    protected readonly IPromptDescriptorFactory _promptDescriptorFactory;
    protected readonly IFunctionDescriptorFactory _functionDescriptorFactory;
    protected readonly ISemanticStore _semanticStore;
    protected readonly IAzureLLMQueryClient _llmQueryClient;
    protected readonly ILogger<State<TTransition, TStepInput>> _logger;
    protected readonly StreamReporter _streamReporter;
    protected readonly IOrchestrationService<TTransition, TStepInput> _orchestrationService;
    protected readonly IStateFactory _stateFactory;
    protected bool _isFirstRun = true;

    protected State(
        IOrchestrationService<TTransition, TStepInput> orchestrationService, 
        IPromptDescriptorFactory promptDescriptorFactory, 
        IFunctionDescriptorFactory functionDescriptorFactory, 
        ISemanticStore semanticStore,
        IAzureLLMQueryClient llmQueryClient, 
        ILogger<State<TTransition, TStepInput>> logger, 
        StreamReporter streamReporter,
        IStateFactory stateFactory)
    {
        _orchestrationService = orchestrationService;
        _promptDescriptorFactory = promptDescriptorFactory;
        _functionDescriptorFactory = functionDescriptorFactory;
        _semanticStore = semanticStore;
        _llmQueryClient = llmQueryClient;
        _logger = logger;
        _streamReporter = streamReporter;
        _stateFactory = stateFactory;
    }

    public virtual string GetName() => throw new InvalidOperationException();

    public virtual Task<(StepResult, TTransition)> HandleState(StateContext<TTransition, TStepInput> context, Session<TTransition, TStepInput> session, TTransition command, TStepInput stepInput)
    {
        throw new InvalidOperationException();
    }

    public StepResultKey GetStepResultKey() => new (GetName(), Session<TTransition, StepInput>.IncrementalResultKeyPostfix);

    protected void AddSemanticContext(string inputText, string outputText)
    {
        _semanticStore.Add(inputText, outputText);
    }

    protected (StepResult, TTransition) TransitionToNextState(
        StateContext<TTransition, TStepInput> context,
        Session<TTransition, TStepInput> session,
        ChatCompletion chatCompletion,
        string instructionsToUserOnDetected,
        string overrideUserMessage,
        State<TTransition, TStepInput> nextState,
        TTransition nextTransition)
    {
        using var activityScope = ActivityScope.Create("State");
        return activityScope.Monitor<(StepResult, TTransition)>(() =>
        {
            context.SetState(nextState);
            session.SetCurrentStep(context.GetCurrentState(), nextTransition);

            // Reset the first run boolean for the next state
            return new(
                new StepResult
                {
                    StepSuccess = true,
                    CoPilotChatResponseMessages = string.IsNullOrEmpty(instructionsToUserOnDetected)
                    ? null
                    : new List<CoPilotChatResponseMessage>
                    {
                        new CoPilotChatResponseMessage(instructionsToUserOnDetected, chatCompletion, true)
                    },
                    OverrideUserMessage = overrideUserMessage,
                    PreviousChatCompletion = chatCompletion
                },
                nextTransition);
        });
    }
}