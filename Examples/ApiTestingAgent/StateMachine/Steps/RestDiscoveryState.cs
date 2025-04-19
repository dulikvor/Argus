using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Xml;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class RestDiscoveryState : State<ApiTestStateTransitions, StepInput, StepResult>
    {
        private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;

        public override string GetName() => nameof(RestDiscoveryState);

        public RestDiscoveryState(
            IGitHubLLMQueryClient gitHubLLMQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory)
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
                case ApiTestStateTransitions.RestDiscovery:
                    {
                        return await RestDiscovery(context, session, stepInput.CoPilotChatRequestMessage);
                    }
                case ApiTestStateTransitions.RawContentGet:
                    {
                        return await GetRawContent(context, session, stepInput.PreviousStepResult.FunctionResponses.First(), stepInput.PreviousStepResult.PreviousChatCompletion);
                    }
                default:
                    {
                        context.OnNonSupportedTransition(transition);
                        return default;
                    }
            }
        }

        private async Task<(StepResult, ApiTestStateTransitions)> GetRawContent(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context, 
            Session<ApiTestStateTransitions, StepInput, StepResult> session,
            FunctionResponse functionResponse,
            ChatCompletion previousChatCompletion)
        {
            var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<string>, string, string, string, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var arguments = concreteFunctionDescriptor.GetParameters<GetGitHubRawContentFunctionDescriptor.GetGitHubRawContentParametersType>(JsonSerializer.Serialize(functionResponse.FunctionArguments));

            string rawContent = default;
            try
            {
                rawContent = await concreteFunctionDescriptor.Function(arguments.User, arguments.Repo, arguments.Branch, arguments.PathToFile);
            }
            catch (HttpResponseException exception)
            {
                var errorMessage = $"route used {arguments.User}/{arguments.Repo}/{arguments.Branch}/{arguments.PathToFile}, returned status code {exception.StatusCode}";
                session.SetCurrentStep(this, ApiTestStateTransitions.RestDiscovery);
                return new(
                 new StepResult
                 {
                     StepSuccess = false,
                     CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                     {
                         new CoPilotChatResponseMessage($"The GetRawContent function failed to execute. {errorMessage}", previousChatCompletion, false)
                     }
                 },
                 ApiTestStateTransitions.RestDiscovery);
            }

            session.AddStepResult(new(GetName(), string.Format(StatePromptsConstants.SessionResult.SessionResultFunctionFormat, concreteFunctionDescriptor.ToolDefinition.FunctionName)), rawContent);
            session.SetCurrentStep(this, ApiTestStateTransitions.RestDiscovery);
            return new(
                    new StepResult
                    {
                        StepSuccess = true,
                    },
                    ApiTestStateTransitions.RestDiscovery);
        }

        private async Task<(StepResult, ApiTestStateTransitions)> RestDiscovery(
            StateContext<ApiTestStateTransitions, StepInput, StepResult> context,
            Session<ApiTestStateTransitions, StepInput, StepResult> session, 
            CoPilotChatRequestMessage coPilotChatRequestMessage)
        {
            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(RestDiscoveryPromptDescriptor));
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey));

            var structuredOutput = new OpenAIStructuredOutput(
                nameof(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey),
                concretePromptDescriptor.GetStructuredResponse(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey));

            var concreteFunctionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

            var chatCompletionResponse = await _gitHubLLMQueryClient.Query<RestDiscoveryOutput>(coPilotChatRequestMessage, structuredOutput, new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

            if(chatCompletionResponse.IsToolCall)
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
                    var resourceAsString = string.Join(',', restDiscovery.DetectedResources.Select(x => x.ToString()));
                    session.AddStepResult(new(GetName(), string.Format(StatePromptsConstants.SessionResult.SessionResultFunctionFormat, concreteFunctionDescriptor.ToolDefinition.FunctionName)), resourceAsString);
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
}
