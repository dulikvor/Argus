using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class RestDiscoveryPromptDescriptor : BaseStatePromptDescriptor
{
    public override string DescriptorType => nameof(RestDiscoveryPromptDescriptor);

    public RestDiscoveryPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(PromptsConstants.CommandDiscovery.Keys.RestSelectPromptKey,
            "You are given or expecting a customer request and a known selected REST API resource, defined by:\n" +
            "- An HTTP method (GET, POST, PUT, DELETE, etc.)\n" +
            "- A full resource path, including all required segments\n" +
            "- A request body structure, if applicable\n\n" +

            "Your task is to match the customer request to one of the known REST operations using the following criteria:\n" +
            "1. The HTTP method and resource path must align with the customer's intent.\n" +
            "2. The known request body must be sufficient to fulfill the customer's request.\n" +
            "3. All resource names required to complete the URI must be known (i.e., the user has provided them).\n\n" +

            "If a match is found, return:\n" +
            "- The selected HTTP method\n" +
            "- The complete resource URI (with all resource segments filled in)\n" +
            "- The full request content (in JSON format) required to fulfill the operation\n\n" +

            "If no match can be found, return an error message indicating one of the following failure conditions:\n" +
            "- Not all required resource names are known, so the URI cannot be constructed\n" +
            "- No matching REST route exists for the requested method and path\n" +
            "- The known request body cannot fulfill the customer's request\n\n" +

            "Output format if matched:\n" +
            "{\n" +
            "  \"method\": \"POST\",\n" +
            "  \"path\": \"/subscriptions/123/resourceGroups/my-rg/providers/Microsoft.Example/examples/example123\",\n" +
            "  \"requestBody\": {\n" +
            "    \"propertyA\": \"value1\",\n" +
            "    \"propertyB\": \"value2\"\n" +
            "  }\n" +
            "}\n\n" +

            "Output format if not matched:\n" +
            "{\n" +
            "  \"error\": \"No matching REST route found, or request cannot be fulfilled with known content.\"\n" +
            "}");

        // Initialize structured responses
        var restSelectedReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                restCompileIsValid = new { type = "boolean", description = "Whether customer rest request was understood and selected in accordance." },
                instructionsToUser = new { type = "string", description = "Instructions for the user in case the rest selection is not included/something is missing or not successfully concluded." },
                fullResourceUri = new { type = "string", description = "The full resource uri of the selected rest resource." },
                httpMethod = new { type = "string", description = "The supported HTTP method for the rest resource." },
                requestContent = new { type = "string", description = "The proposed request content in JSON format matched to fulfill the customer request." }
            },
            required = new[] { "restDiscoveryIsValid", "instructionsToUser", "fullResourceUri", "httpMethod", "requestContent" }
        };

        StructuredResponses.Add<RestDiscoveryOutput>(PromptsConstants.CommandDiscovery.Keys.RestSelectReturnedOutputKey,
            JsonSerializer.Serialize(restDiscoveryReturnedOutputSchema));
    }
}
