using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.StructuredResponses;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Text.Json;
using StepResultKey = (string, string);

namespace Argus.Common.Orchestration
{
    public class OrchestrationService<TTransition, TStepInput> : IOrchestrationService<TTransition, TStepInput> 
        where TTransition : Enum
        where TStepInput : StepInput
    {
        private readonly IPromptDescriptorFactory _promptDescriptorFactory;
        private readonly ISemanticStore _semanticStore;
        private readonly IAzureLLMQueryClient _llmQueryClient;
        private readonly ILogger<OrchestrationService<TTransition, TStepInput>> _logger;

        public OrchestrationService(
        IPromptDescriptorFactory promptDescriptorFactory,
        ISemanticStore semanticStore,
        IAzureLLMQueryClient llmQueryClient,
        ILogger<OrchestrationService<TTransition, TStepInput>> logger)
        {
            _promptDescriptorFactory = promptDescriptorFactory;
            _semanticStore = semanticStore;
            _llmQueryClient = llmQueryClient;
            _logger = logger;
        }

        public Task<(StepResult, TTransition)> DetectAndConfirm<TOutput>(
                Session<TTransition, TStepInput> session,
                TStepInput stepInput,
                StepResultKey stepResultKey,
                ChatCompletionStructuredResponse<TOutput> chatCompletionStructuredResponse,
                Func<TOutput, bool> isDetected,
                Func<TOutput, string> getConfirmationMessage,
                TTransition retryTransition,
                bool withSoftConsent = false,
                bool withStepResult = true)
                where TOutput : BaseOutput
        {
            using var activityScope = ActivityScope.Create("OrchestrationService");
            return activityScope.Monitor<(StepResult, TTransition)>(() =>
            {
                var structuredOutput = chatCompletionStructuredResponse.StructuredOutput;
                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;

                if (isDetected(structuredOutput))
                {
                    if (withStepResult)
                    {
                        session.AddStepResult(stepResultKey, structuredOutput.OutputIncrementalResult());
                    }
                    _semanticStore.Add(coPilotChatRequestMessage.GetUserFirstAsPlainText(), structuredOutput.InstructionsToUserOnDetected());
                    var confirmation = CopilotConfirmationRequestMessage.GenerateConfirmationData();
                    session.SetCurrentConfirmationId(confirmation.Id);
                    if (withSoftConsent == false)
                    {
                        return Task.FromResult<ValueTuple<StepResult, TTransition>>(
                            new(
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
                                retryTransition
                            )
                        );
                    }

                }

                return Task.FromResult<ValueTuple<StepResult, TTransition>>(
                    new(
                        new StepResult
                        {
                            StepSuccess = false,
                            CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                            {
                            new CoPilotChatResponseMessage(structuredOutput.InstructionsToUserOnDetected(), chatCompletionStructuredResponse.ChatCompletion, false)
                            },
                            PreviousChatCompletion = chatCompletionStructuredResponse.ChatCompletion
                        },
                        retryTransition
                    )
                );
            });
        }

        public async Task<ChatCompletionStructuredResponse<TOutput>> QueryLLM<TOutput>(
            CoPilotChatRequestMessage requestMessage,
            string promptDescriptorName,
            string promptKey,
            string outputKey,
            List<ChatTool> tools,
            bool withContext = true)
            where TOutput : BaseOutput
        {
            using var activityScope = ActivityScope.Create("OrchestrationService");
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
                    _logger.LogInformation("Message{Index}: {Content} {@Meta}", index, item, new { Scope = "State" });
                }

                var structuredOutput = new OpenAIStructuredOutput(outputKey, concretePromptDescriptor.GetStructuredResponse(outputKey));

                // Use Polly retry for LLM call
                var chatCompletionResponse = await this.WithRetry<ChatCompletionStructuredResponse<TOutput>, JsonException>(
                    async () => await _llmQueryClient.Query<TOutput>(requestMessage, structuredOutput, tools)
                );

                return chatCompletionResponse;
            });
        }

        public async Task<(bool IsConsentGiven, ConsentAction Action, ChatCompletion ChatCompletion)> CheckCustomerConsent(
            Session<TTransition, TStepInput> session,
            TStepInput stepInput)
        {
            using var activityScope = ActivityScope.Create("OrchestrationService");
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

        private async Task SetContext(CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            using var activityScope = ActivityScope.Create("OrchestrationService");
            await activityScope.Monitor(async () =>
            {
                var sematicResult = await _semanticStore.Search(coPilotChatRequestMessage.GetUserFirstAsPlainText());

                var concretePromptDescriptor = (ContextPromptDescriptor)_promptDescriptorFactory.GetPromptDescriptor(nameof(ContextPromptDescriptor));
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.ReconcilePrompt(sematicResult), SystemMessagePriority.Low);
            });
        }
    }
}
