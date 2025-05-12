using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class ExpectedOutcomePromptDescriptor : BaseStatePromptDescriptor
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
            "You are tasked with defining the expected outcome for a REST API command.\n" +
            "The user will provide details such as HTTP status, expected errors, or properties of the returned response to validate.\n" +
            "1. Confirm the expected outcome with the user.\n" +
            "2. Ensure the expectations are clear and actionable.\n\n" +
            "Provide a structured response summarizing the expected outcome.";

        var expectedOutcomeReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                stepIsConcluded = new { type = "boolean", description = "Whether the step was concluded successfully." },
                expectedHttpStatusCode = new { type = "integer", description = "The expected HTTP status code for the API response." },
                expectedErrorMessage = new { type = "string", description = "The expected error message, if any." },
                expectedResponseContent = new { type = "string", description = "The expected content of the API response, if any." }
            },
            required = new[] { "stepIsConcluded" }
        };

        StructuredResponses.Add<ExpectedOutcomeOutput>(PromptsConstants.ExpectedOutcome.Keys.ExpectedOutcomeReturnedOutputKey,
            JsonSerializer.Serialize(expectedOutcomeReturnedOutputSchema));
    }
}