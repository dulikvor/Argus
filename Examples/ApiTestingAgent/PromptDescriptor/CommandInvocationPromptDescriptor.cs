using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class CommandInvocationPromptDescriptor : BaseStatePromptDescriptor
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
            "1. Confirm the expected outcome with the user.\n" +
            "2. Execute the REST API command.\n" +
            "3. Compare the actual response with the expected outcome.\n" +
            "4. Provide a detailed analysis of what worked, what didn’t, and why.\n\n" +
            "Ensure the user’s expectations are clearly understood and met.";

        var commandInvocationReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                stepIsConcluded = new { type = "boolean", description = "Whether the step was concluded successfully." },
                analysis = new { type = "string", description = "Detailed analysis of the command invocation outcome." },
                actualResponse = new { type = "object", description = "The actual response returned by the REST API." },
                expectedOutcome = new { type = "object", description = "The expected outcome as defined by the user." }
            },
            required = new[] { "stepIsConcluded", "analysis", "actualResponse", "expectedOutcome" }
        };

        StructuredResponses.Add<CommandInvocationOutput>(PromptsConstants.CommandInvocation.Keys.CommandInvocationReturnedOutputKey,
            JsonSerializer.Serialize(commandInvocationReturnedOutputSchema));
    }
}