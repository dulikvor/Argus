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
using ApiTestingAgent.Services;
using static ApiTestingAgent.PromptDescriptor.PromptsConstants;

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
                        return await RestDiscovery(context, session, stepInput.CoPilotChatRequestMessage, stepInput.PreviousStepResult.PreviousChatCompletion);
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
            var toolArguments = $"{arguments.User}/{arguments.Repo}/{arguments.Branch}/{arguments.PathToFile}";
            try
            {
                rawContent = await concreteFunctionDescriptor.Function(arguments.User, arguments.Repo, arguments.Branch, arguments.PathToFile);
            }
            catch (HttpResponseException exception)
            {
                var errorMessage = $"route used {toolArguments}, returned status code {exception.StatusCode}";
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

            session.AddStepResult(new(GetName(), string.Format(PromptsConstants.SessionResult.Formats.SessionResultFunctionSourceArgumentsFormat, concreteFunctionDescriptor.ToolDefinition.FunctionName)), toolArguments);
            session.AddStepResult(new(GetName(), string.Format(PromptsConstants.SessionResult.Formats.SessionResultFunctionFormat, concreteFunctionDescriptor.ToolDefinition.FunctionName)), rawContent);
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
            CoPilotChatRequestMessage coPilotChatRequestMessage,
            ChatCompletion previousChatCompletion)
        {

            var confirmationState = coPilotChatRequestMessage.GetUserFirst()?.GetConfirmation(session.CurrentConfirmationId);
            if (confirmationState == ConfirmationState.Accepted)
            {
                session.ResetConfirmationId();
                context.SetState(new CommandDiscoveryState(_gitHubLLMQueryClient, _promptDescriptorFactory, _functionDescriptorFactory));
                session.SetCurrentStep(context.GetCurrentState(), ApiTestStateTransitions.CommandDiscovery);
                return new(
                        new StepResult
                        {
                            StepSuccess = true,
                            CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                            {
                                    new CoPilotChatResponseMessage("rest approved.", previousChatCompletion, false)
                            }
                        },
                        ApiTestStateTransitions.CommandDiscovery);
            }

            var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(RestDiscoveryPromptDescriptor));
            coPilotChatRequestMessage.AddSystemMessage(concretePromptDescriptor.GetPrompt(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey));

            var structuredOutput = new OpenAIStructuredOutput(
                nameof(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey),
                concretePromptDescriptor.GetStructuredResponse(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey));

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
                var sessionResources = (session as ApiTestSession).DetectedResources;
                if (restDiscovery.RestDiscoveryDetectedInCurrentIteration)
                {
                    // Update session on iteration only if results are detected
                    if (sessionResources != null && restDiscovery.DetectedResources.Any())
                    {
                        sessionResources.MergeOrUpdate(restDiscovery.DetectedResources);
                    }
                }

                if (sessionResources != null && restDiscovery.DetectedResources.Any())
                {
                    var confirmation = CopilotConfirmationRequestMessage.GenerateConfirmationData();
                    session.SetCurrentConfirmationId(confirmation.Id);
                    return new(
                            new StepResult
                            {
                                StepSuccess = false,
                                ConfirmationMessage = new CopilotConfirmationRequestMessage
                                {
                                    Title = "Confirm Detected Resources",
                                    Message = restDiscovery.OutputResult(),
                                    Confirmation = confirmation,
                                },
                                PreviousChatCompletion = chatCompletionResponse.ChatCompletion
                            },
                            ApiTestStateTransitions.RestDiscovery);
                }

                return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>()
                        {
                            new CoPilotChatResponseMessage(restDiscovery.OutputIncrementalResult(), chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.RestDiscovery);
            }
                
        }
    }
}
