using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.PromptDescriptor;
using Argus.Common.Functions;
using Argus.Common.Orchestration;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.Telemetry;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Microsoft.Extensions.Logging;

namespace Argus.Common.StateMachine
{
    public class EndState<TTransition, TStepInput> : State<TTransition, TStepInput>
        where TTransition : Enum
        where TStepInput : StepInput
    {
        public override string GetName() => "EndState";

        public EndState(
            IOrchestrationService<TTransition, TStepInput> orchestrationService,
            IPromptDescriptorFactory promptDescriptorFactory, 
            IFunctionDescriptorFactory functionDescriptorFactory,
            ISemanticStore semanticStore,
            IAzureLLMQueryClient llmQueryClient,
            ILogger<State<TTransition, TStepInput>> logger,
            StreamReporter streamReporter,
            IStateFactory stateFactory)
            : base(orchestrationService, promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger, streamReporter, stateFactory)
        {
        }

        public override async Task<(StepResult, TTransition)> HandleState(
            StateContext<TTransition, TStepInput> context,
            Session<TTransition, TStepInput> session,
            TTransition transition,
            TStepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(EndState<TTransition, TStepInput>));
            return await activityScope.Monitor<(StepResult, TTransition)>(async () =>
            {
                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(EndPromptDescriptor));

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;
                coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.Prompts.Keys.EndState), SystemMessagePriority.High);

                var chatCompletionResponse = await _llmQueryClient.Query(coPilotChatRequestMessage);

                return new(
                        new StepResult
                        {
                            StepSuccess = false,
                            CoPilotChatResponseMessages = chatCompletionResponse
                        },
                        default
                    );
            });
        }
    }
}
