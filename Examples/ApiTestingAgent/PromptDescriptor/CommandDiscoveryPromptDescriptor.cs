using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class CommandDiscoveryPromptDescriptor : BaseStatePromptDescriptor
{
    public override string DescriptorType => nameof(CommandDiscoveryPromptDescriptor);

    public CommandDiscoveryPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts[PromptsConstants.CommandDiscovery.Keys.RestSelectPromptKey] =
            "Analyze the provided REST API operations and select the most appropriate command to execute. " +
            "Ensure the command is valid and adheres to the constraints of HTTP methods (GET, POST, PUT, DELETE). " +
            "Selection must strictly follow the customer's specific instructions. Do not make assumptions or guesses. " +
            "The request URI must be one of the REST APIs known through the Rest Discovery step. " +
            "When building the command, review what is already known about the specified command and extend it in accordance with customer requests. " +
            "Valid extensions include changing placeholders in the request URI and updating the request content. " +
            "If any required details are missing, such as the full resource URI or content, instruct the user to provide them. " +
            "For example, if a resource URI is incomplete, ask the user to specify the missing parts (e.g., 'Provide the full path to the resource, including the domain, endpoint, and query parameters if applicable'). " +
            "If the content does not match the expected format, provide examples of the correct format and request the user to update it accordingly. " +
            "If no valid command is found, provide detailed feedback to the user.";

        // Initialize structured responses
        var commandDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                commandIsValid = new { type = "boolean", description = "Whether the command is valid." },
                selectedCommand = new { type = "string", description = "The selected command to execute." },
                instructionsToUser = new { type = "string", description = "Instructions for the user in case the command is invalid or missing." },
                httpMethod = new { type = "string", description = "The HTTP method of the selected command." },
                requestUri = new { type = "string", description = "The request URI of the selected command." },
                content = new { type = "string", description = "The JSON content of the selected command." }
            },
            required = new[] { "commandIsValid", "selectedCommand", "instructionsToUser", "httpMethod", "requestUri", "content" }
        };

        StructuredResponses.Add<CommandDiscoveryOutput>(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey,
            JsonSerializer.Serialize(commandDiscoveryReturnedOutputSchema));
    }
}