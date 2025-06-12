using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StructuredResponses;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using Azure;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

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
    protected bool _isFirstRun = true;

    protected State(IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory, ISemanticStore semanticStore, IAzureLLMQueryClient llmQueryClient, ILogger<State<TTransition, TStepInput>> logger)
    {
        _promptDescriptorFactory = promptDescriptorFactory;
        _functionDescriptorFactory = functionDescriptorFactory;
        _semanticStore = semanticStore;
        _llmQueryClient = llmQueryClient;
        _logger = logger;
    }

    public virtual string GetName() => throw new InvalidOperationException();

    public virtual Task<(StepResult, TTransition)> HandleState(StateContext<TTransition, TStepInput> context, Session<TTransition, TStepInput> session, TTransition command, TStepInput stepInput)
    {
        throw new InvalidOperationException();
    }

    protected async Task SetContext(CoPilotChatRequestMessage coPilotChatRequestMessage)
    {
        using var activityScope = ActivityScope.Create("State");
        await activityScope.Monitor(async () =>
        {
            var sematicResult = await _semanticStore.Search(coPilotChatRequestMessage.GetUserFirstAsPlainText());

            var concretePromptDescriptor = (ContextPromptDescriptor)_promptDescriptorFactory.GetPromptDescriptor(nameof(ContextPromptDescriptor));
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.ReconcilePrompt(sematicResult), SystemMessagePriority.Low);
        });
    }

    protected (StepResult, TTransition) DetectAndConfirm<TOutput>(
                Session<TTransition, TStepInput> session,
                TStepInput stepInput,
                ChatCompletionStructuredResponse<TOutput> chatCompletionStructuredResponse,
                Func<TOutput, bool> isDetected,
                Func<TOutput, string> getConfirmationMessage,
                TTransition retryTransition,
                bool withSoftConsent = false,
                bool withStepResult = true)
                where TOutput : BaseOutput
    {
        using var activityScope = ActivityScope.Create("State");
        return activityScope.Monitor<(StepResult, TTransition)>(() =>
        {
            var structuredOutput = chatCompletionStructuredResponse.StructuredOutput;
            var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;

            if (isDetected(structuredOutput))
            {
                if(withStepResult)
                {
                    session.AddStepResult(new(GetName(), Session<TTransition, TStepInput>.IncrementalResultKeyPostfix), structuredOutput.OutputIncrementalResult());
                }
                _semanticStore.Add(coPilotChatRequestMessage.GetUserFirstAsPlainText(), structuredOutput.InstructionsToUserOnDetected());
                var confirmation = CopilotConfirmationRequestMessage.GenerateConfirmationData();
                session.SetCurrentConfirmationId(confirmation.Id);
                if (withSoftConsent == false)
                {
                    return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        ConfirmationMessage = new CopilotConfirmationRequestMessage
                        {
                            Title = "Confirm Detected",
                            Message = getConfirmationMessage(structuredOutput),
                            Confirmation = confirmation
                        },
                        PreviousChatCompletion = chatCompletionStructuredResponse.ChatCompletion
                    },
                    retryTransition);
                }

            }

            return new(
                new StepResult
                {
                    StepSuccess = false,
                    CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                    {
                        new CoPilotChatResponseMessage(structuredOutput.InstructionsToUserOnDetected(), chatCompletionStructuredResponse.ChatCompletion, false)
                    },
                    Message = structuredOutput.InstructionsToUserOnDetected()
                },
                retryTransition);
        });
    }

    protected async Task<ChatCompletionStructuredResponse<TOutput>> QueryLLM<TOutput>(
            CoPilotChatRequestMessage requestMessage,
            string promptDescriptorName,
            string promptKey,
            string outputKey,
            List<ChatTool> tools,
            bool withContext = true)
            where TOutput : BaseOutput
    {
        using var activityScope = ActivityScope.Create("State");
        return await activityScope.Monitor(async () =>
        {
            if (withContext)
            {
                await SetContext(requestMessage);
            }

            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(promptDescriptorName);
            requestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(promptKey), SystemMessagePriority.High);

            var requestMessagesContent = requestMessage.GetMessagesContent();
            
            // Log each message content for tracing/debugging
            foreach (var (item, index) in requestMessagesContent.Select((item, index) => (item, index)))
            {
                _logger.LogInformation("Message{Index}: {Content} {@Meta}", index, item, new { Scope = "State"});
            }   
            
            var structuredOutput = new OpenAIStructuredOutput(outputKey, concretePromptDescriptor.GetStructuredResponse(outputKey));
            var chatCompletionResponse = await _llmQueryClient.Query<TOutput>(requestMessage, structuredOutput, tools);

            return chatCompletionResponse;
        }); 
    }

    protected void AddSemanticContext(string inputText, string outputText)
    {
        _semanticStore.Add(inputText, outputText);
    }

    public async Task<(StepResult, TTransition)> Introduction(CoPilotChatRequestMessage coPilotChatRequestMessage, TTransition nextTransition)
    {
        using var activityScope = ActivityScope.Create("State");
        return await activityScope.Monitor<(StepResult, TTransition)>(async () =>
        {
            if (_isFirstRun)
            {
                var currentStatePromptDescriptor = (CurrentStatePromptDescriptor)_promptDescriptorFactory.GetPromptDescriptor(nameof(CurrentStatePromptDescriptor));
                coPilotChatRequestMessage.AddSystemMessage(currentStatePromptDescriptor.GetPrompt(PromptsConstants.Prompts.Keys.CurrentState), SystemMessagePriority.High);

                // Query the LLM with the current state prompt
                var chatCompletionResponse = await _llmQueryClient.Query<string>(coPilotChatRequestMessage, null, null);

                _isFirstRun = false;

                return new(
                new StepResult
                {
                    StepSuccess = false,
                    CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                    {
                    new CoPilotChatResponseMessage(chatCompletionResponse.StructuredOutput, chatCompletionResponse.ChatCompletion, false)
                    }
                },
                nextTransition);
            }

            return (null, nextTransition);
        });
    }

    protected (StepResult, TTransition) TransitionToNextState(
        StateContext<TTransition, TStepInput> context,
        Session<TTransition, TStepInput> session,
        ChatCompletion previousChatCompletion,
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
                    StepSuccess = true
                },
                nextTransition);
        });
    }

    protected async Task<(bool IsConsentGiven, ConsentAction Action, ChatCompletion ChatCompletion)> CheckCustomerConsent(
            Session<TTransition, TStepInput> session,
            TStepInput stepInput)
    {
        using var activityScope = ActivityScope.Create("State");
        return await activityScope.Monitor<(bool IsConsentGiven, ConsentAction Action, ChatCompletion ChatCompletion)>(async () =>
        {
            // Ensure there is a pending confirmation in the session
            if (!session.HasPendingConfirmation())
            {
                return (false, ConsentAction.Unknown, null);
            }

            // Prepare the request message
            var requestMessage = stepInput.CoPilotChatRequestMessage.GetUserFirst();

            // Query the LLM using the consent prompt
            var chatCompletionResponse = await QueryLLM<CustomerConsentStateTransitionResponse>(
                requestMessage,
                nameof(CustomerConsentStateTransitionPromptDescriptor),
                PromptsConstants.Prompts.Keys.CustomerConsentStateTransition,
                PromptsConstants.StructuredResponses.Keys.CustomerConsentStateTransitionResponseSchema,
                null,
                false
            );

            // Extract the structured response
            var structuredResponse = chatCompletionResponse.StructuredOutput;

            // Log or handle the reasoning for debugging purposes
            if (!string.IsNullOrEmpty(structuredResponse.Reasoning))
            {
                Console.WriteLine($"Reasoning provided by LLM: {structuredResponse.Reasoning}");
            }

            if (structuredResponse.Action == ConsentAction.ConsentApproval)
            {
                session.ResetConfirmationId();
            }

            // Determine if consent is given based on the structured response
            bool isConsentGiven = structuredResponse.Action == ConsentAction.ConsentApproval && structuredResponse.ApprovalStatus == ApprovalStatus.Approved;

            return (isConsentGiven, structuredResponse.Action, chatCompletionResponse.ChatCompletion);
        });
    }
}