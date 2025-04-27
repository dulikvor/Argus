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
        Prompts[PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey] =
            "You are given an OpenAPI JSON specification for Azure REST API resources.\n" +
            "Analyze all operations under the `paths` section and extract for each:\n" +
            "1. **REST route** – full API URL with placeholders (e.g., `{subscriptionId}`, `{resourceGroupName}`).\n" +
            "2. **HTTP method** – (GET, PUT, POST, DELETE, PATCH, etc.).\n" +
            "3. **Request content** – full JSON body if required; skip if not applicable.\n\n" +
            "If updated request structures or new URIs are provided, prioritize them over historic data. Request tool action to analyze new URIs if needed.\n\n" +
            "Ensure no operations are missed, including migrations, batch processes, and uncommon methods like `PATCH`.\n\n" +
            "Return the results in clean, labeled **Markdown** format. Focus strictly on extracting REST route, HTTP method, and request body (if any).\n\n" +
            "If a valid specification URL or page is missing, prompt the user to provide one.\n\n" +
            "Present the findings for **user approval** before marking as valid. Accept corrections or additional input if offered.";

        var restDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                stepIsConcluded = new { type = "boolean", description = "Whether the step was concluded. This will be explicitly false until the customer approves the rest contract, if approved - should be true." },
                restDiscoveryDetectedInCurrentIteration = new { type = "boolean", description = "Whether rest discovery was detected in the current iteration. This will be explicitly false if no resources were detected due to the customer's recent message, such as a legitimate lead for a method, an explicit change stated by the customer, or unrelated queries where resources are returned only due to historic state." },
                instructionsToUser = new
                {
                    type = "string",
                    description = "Instructions for the user in case the rest discovery is not included or successfully concluded. If known resources exist, they will be presented along with instructions on what is required for approval."
                },
                detectedResources = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        description = "Details of a detected REST resource.",
                        properties = new
                        {
                            resourceDepiction = new { type = "string", description = "The full depiction of the REST resource." },
                            httpMethod = new { type = "string", description = "The supported HTTP method for the resource." },
                            supportedJsonContent = new { type = "string", description = "The supported JSON content for the request." }
                        },
                        required = new[] { "resourceDepiction", "httpMethod", "supportedJsonContent" }
                    }
                }
            },
            required = new[] { "restDiscoveryDetectedInCurrentIteration", "stepIsConcluded", "instructionsToUser", "detectedResources" }
        };

        StructuredResponses.Add<RestDiscoveryOutput>(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey,
            JsonSerializer.Serialize(restDiscoveryReturnedOutputSchema));
    }
}
