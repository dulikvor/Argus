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
            "### CURRENT STATE & GOAL\n\n" +
            "State: CommandInvocationState\n\n" +
            "Notes:\n" +
            "You are tasked with reviewing the result of a REST API command execution.\n" +
            "The user will provide the expected outcome (such as HTTP status, error presence, or specific response content).\n" +
            "You will receive the actual invocation result (status, error, content) as context.\n\n" +
            "1. Compare the actual result with the expected outcome.\n" +
            "2. If everything matches:\n" +
            "   - Set stepIsConcluded to true.\n" +
            "   - Provide a clear, concise summary of the result confirming success.\n" +
            "3. If there is any mismatch:\n" +
            "   - Set stepIsConcluded to false.\n" +
            "   - Explain in detail what was expected vs what was received.\n" +
            "   - Provide a human-readable recommendation for what the user should try next to achieve the expected result.\n" +
            "   - Do not suggest automated tool calls or continue execution steps.\n" +
            "4. Always include:\n" +
            "   - The current status: what is known (command, expected outcome, actual result).\n" +
            "   - The current state: what phase we’re in (e.g., comparing result, waiting for input).\n" +
            "   - A clear analysis to help the user understand and correct the issue manually.\n\n" +
            "Your output must include only two fields:\n" +
            "- stepIsConcluded (true/false)\n" +
            "- analysis (a plain-language explanation and recommendation)\n\n" +
            "Do not include tool calls or execution plans. Focus on actionable advice and clarity for the user.";

        // Add a prompt to explain the result of a REST API invocation to the user
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationHttpResultExplanationPromptKey] =
            "You are to explain the result of a REST API invocation to the user.\n" +
            "Given the HTTP status code and the content returned by the API, provide a clear, concise explanation of what the result means.\n" +
            "If the status indicates success, summarize the returned content.\n" +
            "If the status indicates an error, explain the error and what it means for the user.\n" +
            "Always use user-friendly language and avoid technical jargon where possible.";

        // Add a prompt to detect user intent for state transitions
        Prompts[PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStatePromptKey] =
            "Analyze the user's input and determine the next workflow state using the following rules:\n" +
            "1. If the user explicitly requests to modify the selected command (e.g., URI, method, or body), return \"CommandSelect\".\n" +
            "2. If the user explicitly requests to modify the expected outcome (e.g., status code, expected content, or error), return \"ExpectedOutcome\".\n" +
            "3. If no selected command has been defined yet, return \"CommandSelect\".\n" +
            "4. If no expected outcome has been defined yet, return \"ExpectedOutcome\".\n" +
            "5. If the user explicitly indicates a desire to proceed (e.g., \"run the command\", \"go ahead\", etc.) and both the command and expected outcome are already known, return \"None\".\n\n" +
            "Important:\n" +
            "- Do **not** infer user intent from context or previous messages.\n" +
            "- Only change state when the user **explicitly** says so.\n" +
            "- Do not assume the user wants to change anything unless clearly stated.\n\n" +
            "Always include a `currentStatus` field summarizing:\n" +
            "- The selected command: full URI, HTTP method, and content (if any).\n" +
            "- The expected outcome: status code, content, and error (if any).\n\n" +
            "Also include a `reasoning` field explaining why you selected the `nextState`, based strictly on explicit user input.\n\n" +
            "Respond with a JSON object with exactly these three fields:\n" +
            "- \"nextState\": one of \"CommandSelect\", \"ExpectedOutcome\", or \"None\".\n" +
            "- \"currentStatus\": concise, user-friendly summary.\n" +
            "- \"reasoning\": brief explanation referencing the user's explicit input.\n\n" +
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
                currentStatus = new { type = "string", description = "A summary of all steps and known information (current status)." },
                reasoning = new { type = "string", description = "A brief explanation of the logic and evidence leading to the nextState decision." }
            },
            required = new[] { "nextState", "currentStatus", "reasoning" }
        };

        StructuredResponses.Add<CommandInvocationDetectNextStateOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationDetectNextStateOutputKey,
            JsonSerializer.Serialize(commandInvocationDetectNextStateOutputSchema));
    }
}