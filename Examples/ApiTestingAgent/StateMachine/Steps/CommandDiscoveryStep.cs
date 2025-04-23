using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandDiscoveryState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(CommandDiscoveryState);

        public CommandDiscoveryState(IGitHubLLMQueryClient gitHubLLMQueryClient, IPromptDescriptorFactory promptDescriptorFactory, IFunctionDescriptorFactory functionDescriptorFactory)
            :base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context, 
            Session<ApiTestStateTransitions, StepInput, StepResult> session, 
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            switch (transition)
            {
                case ApiTestStateTransitions.RestCompile:
                    {
                        return await RestCompile(context, session, stepInput.CoPilotChatRequestMessage);
                    }
                default:
                    {
                        context.OnNonSupportedTransition(transition);
                        return default;
                    }
            }
        }

        private async Task<(StepResult, ApiTestStateTransitions)> RestCompile(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context,
            Session<ApiTestStateTransitions, StepInput, StepResult> session,
            CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(RestDiscoveryPromptDescriptor));
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey));

            var structuredOutput = new OpenAIStructuredOutput(
                nameof(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey),
                concretePromptDescriptor.GetStructuredResponse(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey));

            var concreteFunctionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query<RestDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput, new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

            if (chatCompletionResponse.IsToolCall)
            {
                return new(
                    new StepResult
                    {
                        StepSuccess = true,
                        FunctionResponses = chatCompletionResponse.FunctionResponses,
                        PreviousChatCompletion = chatCompletionResponse.ChatCompletion
                    },
                    ApiTestStateTransitions.RawContentGet
                );
            }
            else
            {
                var restDiscovery = chatCompletionResponse.StructuredOutput;
                if (restDiscovery.RestDiscoveryIsValid)
                {
                    session.AddStepResult(new(GetName(), string.Format(PromptsConstants.SessionResult.SessionResultFunctionFormat, concreteFunctionDescriptor.ToolDefinition.FunctionName)), restDiscovery.DetectedResources);
                    session.AddStepResult(new(GetName(), $"Rest Discovery Resources Found"), restDiscovery.ToString());
                    context.SetState(new EndState<ApiTestStateTransitions, StepInput>(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                    session.SetCurrentStep(context.GetCurrentState(), ApiTestStateTransitions.Any);
                }

                return new(
                    new StepResult
                    {
                        StepSuccess = restDiscovery.RestDiscoveryIsValid,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>() { new CoPilotChatResponseMessage(restDiscovery.ToString(), chatCompletionResponse.ChatCompletion, false) }
                    },
                    ApiTestStateTransitions.TestDescriptor
                );
            }
        }
}
