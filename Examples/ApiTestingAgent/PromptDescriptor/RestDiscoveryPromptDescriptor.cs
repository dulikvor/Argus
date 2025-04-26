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
            "You are given an OpenAPI JSON specification containing Azure REST API resources. " +
            "Your task is to analyze all the operations listed under the \"paths\" section and extract the following for each operation:\n\n" +
            "1. **Rest route** \u2013 the full API endpoint URL (with placeholders like {subscriptionId}, {resourceGroupName}, {workspaceName}, etc.).\n" +
            "2. **HTTP method** \u2013 e.g., GET, PUT, POST, DELETE, PATCH, etc.\n" +
            "3. **Request content** \u2013 show the request body only if required (in JSON format) show it in full, every supported property. If the operation doesn't require a request body, skip this field.\n\n" +
            "If the user provides additional or updated request content structures, dynamically adapt to include these changes in the analysis. Ensure the updated structure is reflected in the detected resources.\n\n" +
            "Ensure that all HTTP methods, including less common ones (like `POST` or `PATCH`), are extracted. Do not miss any resource. Return the result in a clean and readable Markdown format with clear labels for each operation.\n" +
            "Focus only on extracting the REST route, HTTP method, and request content (if applicable).\n\n" +
            "Make sure no resources are overlooked, including special operations like migrations, batch processes, and any other lesser-known operations. Focus on operations under the `paths` section of the specification." +
            "If the URL or HTML page is missing, prompt the user to provide a valid reference that depicts such a Swagger page or documentation." +
            "If the analysis is successful, present the detected resources to the user for approval. The operation cannot be marked as valid until the user explicitly approves the results. If the user wishes to provide additional input or corrections, allow them to do so.";

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
