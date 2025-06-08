using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.Services;
using ApiTestingAgent.StructuredResponses;
using Argus.Clients.LLMQuery;
using Argus.Common.Builtin.Functions;
using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.Clients;
using Argus.Common.Functions;
using Argus.Common.PromptDescriptors;
using Argus.Common.Retrieval;
using Argus.Common.StateMachine;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using Microsoft.Extensions.Primitives;
using OpenAI.Chat;
using OpenTelemetry.Trace;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

using BuiltInPromptsConstants = Argus.Common.Builtin.PromptDescriptor.PromptsConstants;

namespace ApiTestingAgent.StateMachine.Steps
{
    public class RestDiscoveryState : State<ApiTestStateTransitions, StepInput>
    {
        public override string GetName() => nameof(RestDiscoveryState);

        public RestDiscoveryState(
            IAzureLLMQueryClient llmQueryClient,
            IPromptDescriptorFactory promptDescriptorFactory,
            IFunctionDescriptorFactory functionDescriptorFactory, 
            ISemanticStore semanticStore,
            ILogger<State<ApiTestStateTransitions, StepInput>> logger)
            :base(promptDescriptorFactory, functionDescriptorFactory, semanticStore, llmQueryClient, logger)
        {
        }

        public override async Task<(StepResult, ApiTestStateTransitions)> HandleState(
            StateContext<ApiTestStateTransitions, StepInput> context, 
            Session<ApiTestStateTransitions, StepInput> session, 
            ApiTestStateTransitions transition, 
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(RestDiscoveryState));
            return await activityScope.Monitor(async () =>
            {
                switch (transition)
                {
                    case ApiTestStateTransitions.RestDiscovery:
                        {
                            return await RestDiscovery(context, session, stepInput);
                        }
                    case ApiTestStateTransitions.RawContentGet:
                        {
                            return await GetRawContent(context, session, stepInput);
                        }
                    default:
                        {
                            context.OnNonSupportedTransition(transition);
                            return default;
                        }
                }
            });
        }

        private async Task<(StepResult, ApiTestStateTransitions)> GetRawContent(
            StateContext<ApiTestStateTransitions, StepInput> context, 
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(RestDiscoveryState));
            return await activityScope.Monitor<(StepResult, ApiTestStateTransitions)>(async () =>
            {
                var concreteFunctionDescriptor = (ConcreteFunctionDescriptor<Task<string>, string, string, string, string>)_functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

                var arguments = concreteFunctionDescriptor.GetParameters<GetGitHubRawContentFunctionDescriptor.GetGitHubRawContentParametersType>(JsonSerializer.Serialize(stepInput.PreviousStepResult.FunctionResponses.First().FunctionArguments));

                string rawContent = default;
                string errorMessage = default;
                var statusCode = HttpStatusCode.OK;
                var toolArguments = $"{arguments.User}/{arguments.Repo}/{arguments.Branch}/{arguments.PathToFile}";
                try
                {
                    rawContent = await concreteFunctionDescriptor.Function(arguments.User, arguments.Repo, arguments.Branch, arguments.PathToFile);
                }
                catch (HttpResponseException exception)
                {
                    errorMessage = exception.Message;
                    statusCode = exception.StatusCode;
                }

                // Parse Swagger and add detected resources to session
                AddOperationToSession(rawContent, toolArguments, statusCode, errorMessage, (ApiTestSession)session);

                var coPilotChatRequestMessage = stepInput.CoPilotChatRequestMessage.GetUserLast();
                coPilotChatRequestMessage.AddSystemMessage(session.ToString(), SystemMessagePriority.Medium);

                var concretePromptDescriptor = _promptDescriptorFactory.GetPromptDescriptor(nameof(RestDiscoveryPromptDescriptor));
                var chatCompletionResponse = await QueryLLM<StringResponse>(
                    coPilotChatRequestMessage,
                    nameof(RestDiscoveryPromptDescriptor),
                    PromptsConstants.RestDiscovery.Keys.PostRunSwaggerSummaryPromptKey,
                    BuiltInPromptsConstants.StructuredResponses.Keys.StringResponseSchema,
                    null);

                var structuredOutput = chatCompletionResponse.StructuredOutput;
                session.SetCurrentStep(this, ApiTestStateTransitions.RestDiscovery);

                return new(
                    new StepResult
                    {
                        StepSuccess = false,
                        CoPilotChatResponseMessages = new List<CoPilotChatResponseMessage>
                        {
                            new CoPilotChatResponseMessage(structuredOutput.InstructionsToUserOnDetected(), chatCompletionResponse.ChatCompletion, false)
                        }
                    },
                    ApiTestStateTransitions.RestDiscovery);
            });
        }

        private void AddOperationToSession(string rawContent, string toolArguments, HttpStatusCode httpStatusCode, string errorMessage, ApiTestSession session)
        {
            List<Argus.Common.Swagger.SwaggerOperation> operations = null;
            operations = Argus.Common.Swagger.SwaggerParser.ParseOperations(rawContent);
            var apiTestSession = (ApiTestSession)session;
            if (operations?.Any() == true)
            {
                apiTestSession.MergeOrAddResources(operations);
            }

            var sb = new StringBuilder();
            sb.Append($"Detected Rest operations:\n");
            sb.Append($"Route used to get the operations: {toolArguments}\n");
            sb.Append($"Returned HttpStatus: {httpStatusCode}\n");
            sb.Append(string.IsNullOrEmpty(errorMessage) ? string.Empty : $"Returned Error Message: {errorMessage}\n");
            sb.Append($"Rest operations returned:\n");
            apiTestSession.Resources.Select(op => $"Operation HttpMethod: {op.HttpMethod}, Url: {op.Url}").ToList().ForEach(op => sb.AppendLine(op));
            sb.AppendLine();
            session.AddStepResult(new(GetName(), Session<ApiTestStateTransitions, StepInput>.IncrementalResultKeyPostfix), sb.ToString());
        }

        private async Task<(StepResult, ApiTestStateTransitions)> RestDiscovery(
            StateContext<ApiTestStateTransitions, StepInput> context,
            Session<ApiTestStateTransitions, StepInput> session,
            StepInput stepInput)
        {
            using var activityScope = ActivityScope.Create(nameof(RestDiscoveryState));
            return await activityScope.Monitor(async () =>
            {
                var (isConsentGiven, action, chatCompletion) = await CheckCustomerConsent(session, stepInput);
                if (action == ConsentAction.ConsentApproval && isConsentGiven)
                {
                    return TransitionToNextState(
                        context,
                        session,
                        chatCompletion,
                        new CommandInvocationState(_llmQueryClient, _promptDescriptorFactory, _functionDescriptorFactory, _semanticStore, _logger),
                        ApiTestStateTransitions.CommandInvocationAnalysis);
                }

                var concreteFunctionDescriptor = _functionDescriptorFactory.GetFunctionDescriptor(nameof(GetGitHubRawContentFunctionDescriptor));

                var chatCompletionResponse = await QueryLLM<StringResponse>(
                    stepInput.CoPilotChatRequestMessage,
                    nameof(RestDiscoveryPromptDescriptor),
                    PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey,
                    BuiltInPromptsConstants.StructuredResponses.Keys.StringResponseSchema,
                    new List<ChatTool> { concreteFunctionDescriptor.ToolDefinition });

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
                    return DetectAndConfirm(
                        session,
                        stepInput,
                        chatCompletionResponse,
                        _ => ((ApiTestSession)session).ResourcesExists(),
                        output => output.InstructionsToUserOnDetected(),
                        ApiTestStateTransitions.RestDiscovery,
                        true,
                        false);
                }
            });
        }
    }
}
