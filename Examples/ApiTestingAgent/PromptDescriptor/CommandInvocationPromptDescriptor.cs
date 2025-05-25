using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class CommandInvocationPromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(CommandInvocationPromptDescriptor);

    public CommandInvocationPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationPromptKey] =
            "You are tasked with invoking a REST API command.\n" +
            "The user will provide the expected outcome, including HTTP status, error expectations, or properties of the returned response to validate.\n" +
            "You will receive invocation details (status, error, content) as context.\n" +
            "1. Compare the actual invocation details with the expected outcome.\n" +
            "2. If all expectations are met, set stepIsConcluded to true and provide a summary analysis.\n" +
            "3. If any mismatch is found, or if no historic results exist, set stepIsConcluded to false.\n" +
            "   - If you can suggest a new tool call (with required parameters) that could achieve the expected outcome, indicate this in the completion (e.g., set IsToolCall = true), but do not include the tool call in the returned structure.\n" +
            "   - If you cannot provide a proper tool call, return to the user with the current analysis and clear instructions.\n" +
            "4. Always provide a clear analysis summarizing what was expected, what was returned, and what the next step is.\n\n" +
            "Your output must follow the required structure and provide actionable, transparent feedback. The returned structure must only include stepIsConcluded and analysis. If a tool call is needed, indicate this in the completion metadata (e.g., IsToolCall), not in the returned structure.";

        // Add a prompt to explain the result of a REST API invocation to the user
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationHttpResultExplanationPromptKey] =
            "You are to explain the result of a REST API invocation to the user.\n" +
            "Given the HTTP status code and the content returned by the API, provide a clear, concise explanation of what the result means.\n" +
            "If the status indicates success, summarize the returned content.\n" +
            "If the status indicates an error, explain the error and what it means for the user.\n" +
            "Always use user-friendly language and avoid technical jargon where possible.";

        // Add a prompt to detect user intent for state transitions
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStatePromptKey] =
            "Analyze the user's input and determine the next workflow state based on the following:\n" +
            "1. If the user wants to modify the selected command (URI, method, or request content), return \"CommandSelect\".\n" +
            "2. If they want to modify the expected outcome (status, content, or error), return \"ExpectedOutcome\".\n" +
            "3. If no selected command is known (from CommandDiscoveryState), return \"CommandSelect\".\n" +
            "4. If no expected outcome is known (from ExpectedOutcomeState), return \"ExpectedOutcome\".\n" +
            "5. If none apply and the user wants to proceed, return \"None\".\n\n" +
            "Always include a currentStatus field summarizing:\n" +
            "- The selected command: URI (full, including domain and version), HTTP method, and content if any.\n" +
            "- The expected outcome: status code, content, and error (if any).\n\n" +
            "Respond with a JSON object containing only two fields:\n" +
            "- \"nextState\": one of \"CommandSelect\", \"ExpectedOutcome\", or \"None\".\n" +
            "- \"currentStatus\": a concise, user-friendly summary.\n\n" +
            "No other fields or text should be returned.";

        var commandInvocationReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                isExpectedDetected = new { type = "boolean", description = "True if all expectations are met and the step is complete; false if user assistance or a new tool call is needed." },
                analysis = new { type = "string", description = "Summary of the command invocation outcome, including what was expected, what was returned, and what the next step is." }
            },
            required = new[] { "isExpectedDetected", "analysis" }
        };

        StructuredResponses.Add<CommandInvocationOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationReturnedOutputKey,
            JsonSerializer.Serialize(commandInvocationReturnedOutputSchema));

        // Output schema for detecting next state and returning current status
        var commandInvocationDetectNextStateOutputSchema = new
        {
            type = "object",
            properties = new
            {
                nextState = new { type = "string", description = "The next state to transition to. One of: 'CommandSelect', 'ExpectedOutcome', or 'None'." },
                currentStatus = new { type = "string", description = "A summary of all steps and known information (current status)." }
            },
            required = new[] { "nextState", "currentStatus" }
        };

        StructuredResponses.Add<CommandInvocationDetectNextStateOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStateOutputKey,
            JsonSerializer.Serialize(commandInvocationDetectNextStateOutputSchema));
    }
}