using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class RestDiscoveryPromptDescriptor : BasePromptDescriptor
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
            "1. **REST route** � full API URL with placeholders (e.g., `{subscriptionId}`, `{resourceGroupName}`).\n" +
            "2. **HTTP method** � (GET, PUT, POST, DELETE, PATCH, etc.).\n" +
            "3. **Request content** � full JSON body if required; skip if not applicable.\n\n" +
            "If updated request structures or new URIs are provided, prioritize them over historic data. Request tool action to analyze new URIs if needed.\n" +
            "Ensure no operations are missed, including migrations, batch processes, and uncommon methods like `PATCH`.\n\n" +
            "Populate `instructionsToUser` with a message summarizing the findings or explaining what is needed to proceed.\n" +
            "If valid REST resources are detected, include a clear summary of what was found, such as specific REST routes and HTTP methods.\n" +
            "Ask the user to review and either confirm or correct the findings.\n" +
            "If no valid resources are found, explain what is needed to proceed, such as uploading a valid OpenAPI spec or providing additional REST details.\n" +
            "Include helpful examples where appropriate.\n\n" +
            "Return the results in clean, labeled **Markdown** format.";

        var restDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                restDiscoveryDetectedInCurrentIteration = new { type = "boolean", description = "Whether rest discovery was detected in the current iteration. This will be explicitly false if no resources were detected due to the customer's recent message, such as a legitimate lead for a method, an explicit change stated by the customer, or unrelated queries where resources are returned only due to historic state." },
                instructionsToUser = new
                {
                    type = "string",
                    description = "Message shown to the user summarizing the findings or explaining what is needed to proceed. If resources are detected, include a summary of the findings and ask for user confirmation. If no resources are detected, explain what is needed to proceed, such as providing a valid OpenAPI spec or additional REST details. The summary should list detected resources in the format: '- [ResourceDepiction] ([HttpMethod])'."
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
