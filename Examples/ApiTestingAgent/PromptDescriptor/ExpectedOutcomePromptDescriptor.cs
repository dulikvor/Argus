using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class ExpectedOutcomePromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(ExpectedOutcomePromptDescriptor);

    public ExpectedOutcomePromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts[PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomePromptKey] =
            "### CURRENT STATE & GOAL\n\n" +
            "State: ExpectedOutcomeState\n\n" +
            "Notes:\n" +
            "You are tasked with defining the expected outcome for a REST API command.\n\n" +
            "The user will provide details such as HTTP status codes, expected errors, or properties of the returned response to validate.\n\n" +
            "Your responsibilities:\n" +
            "1. Echo back and confirm the user's provided expectations (e.g., HTTP status, content requirements).\n" +
            "2. Ensure the expectations are clear, specific, and actionable.\n" +
            "3. Always present a structured 'Current Status' section that includes:\n" +
            "   - The command being validated (if known, otherwise state 'Not yet provided')\n" +
            "   - The selected and known expected values (e.g., status code, response body fields, error types; use 'Pending' or 'None' if not provided)\n" +
            "   - A clear indication of the current interaction state (e.g., ExpectedOutcomeState, WaitingForUserInput, ConfirmingExpectations)\n\n" +
            "Response Format (always use this):\n" +
            "### ✅ Expected Outcome Summary (ExpectedOutcomeState)\n\n" +
            "**Command:** [Insert known command or 'Not yet provided']\n\n" +
            "**Expected Values:**\n" +
            "- HTTP Status: [Insert or 'Pending']\n" +
            "- Expected Errors: [Insert or 'None']\n" +
            "- Response Properties to Validate: [Insert or 'None specified']\n\n" +
            "**Current State:** ExpectedOutcomeState\n" +
            "[Briefly explain what you're waiting for or what the next step is. If more information is needed from the user, clearly state what is missing.]";

        var expectedOutcomeReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                isExpectedDetected = new { type = "boolean", description = "Whether the expected outcome was detected successfully." },
                expectedHttpStatusCode = new { type = "integer", description = "The expected HTTP status code for the API response." },
                expectedErrorMessage = new { type = "string", description = "The expected error message, if any." },
                expectedResponseContent = new { type = "string", description = "The expected content of the API response, if any." },
                instructionsToUserOnDetectedCommand = new { type = "string", description = "A message for the user that includes the full HTTP method, request URI, content of the detected command, or guidance if no valid command is detected." },
            },
            required = new[] { "isExpectedDetected", "expectedHttpStatusCode", "expectedErrorMessage", "expectedResponseContent", "instructionsToUserOnDetectedCommand" }
        };

        StructuredResponses.Add<ExpectedOutcomeOutput>(PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey,
            JsonSerializer.Serialize(expectedOutcomeReturnedOutputSchema));
    }
}