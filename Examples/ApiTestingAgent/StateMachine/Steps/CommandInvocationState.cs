using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class CommandInvocationState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(CommandInvocationState);

        public CommandInvocationState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory)
            : base(promptDescriptorFactory, functionDescriptorFactory)
        {
            _gitHubLLMQueryClient = gitHubLLMQueryClient;
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context,
            Session<ApiTestStateTransitions, StepInput, StepResult> session,
            ApiTestStateTransitions transition,
            StepInput stepInput)
        {
            if (transition == ApiTestStateTransitions.CommandInvocation)
            {
                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage;

                var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<(int HttpStatusCode, string Content)>, string, string, Dictionary<string, string>, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(RestToolFunctionDescriptor));

                var chatCompletionResponse = await _gitHubLLMQueryClient.Query<string>(
                    coPilotChatRequestMessage,
                    null,
                    new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition },
                    true);

                if (chatCompletionResponse.IsToolCall)
                {
                    var toolArguments = chatCompletionResponse.FunctionResponses.First().FunctionArguments;

                    var arguments = concreteFunctionDescriptor.GetParameters<RestToolFunctionDescriptor.RestToolParametersType>(JsonSerializer.Serialize(toolArguments));
                    var response = await concreteFunctionDescriptor.Function(arguments.Method, arguments.Url, arguments.Headers, arguments.Body);

                    session.AddStepResult(new(GetName(), PromptsConstants.CommandInvocation.Keys.HttpMethod), response.HttpStatusCode);
                    session.AddStepResult(new(GetName(), PromptsConstants.CommandInvocation.Keys.ResponseContent), response.Content);

                    context.SetState(new EndState<ApiTestStateTransitions, StepInput>(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                    session.SetCurrentStep(context.GetCurrentState(), ApiTestStateTransitions.Any);

                    var summaryMessage = $"HTTP Status: {response.HttpStatusCode}, Response: {response.Content}";

                    return new(
                        new StepResult
                        {
                            StepSuccess = true,
                            CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                            {
                                new CoPilotChatResponseMessage(summaryMessage, chatCompletionResponse.ChatCompletion, true)
                            }
                        },
                        ApiTestStateTransitions.Any);
                }

                return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                        {
                            new CoPilotChatResponseMessage("No tool call detected.", chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.CommandDiscovery);
            }

            context.OnNonSupportedTransition(transition);
            return default;
        }
    }
}