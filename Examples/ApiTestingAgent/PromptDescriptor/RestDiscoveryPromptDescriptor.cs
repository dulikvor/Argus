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
        Prompts.Add(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey,
            "You are given an OpenAPI JSON specification containing Azure REST API resources. " +
            "Your task is to analyze all the operations listed under the \"paths\" section and extract the following for each operation:\n\n" +
            "1. **Rest route** – the full API endpoint URL (with placeholders like {subscriptionId}, {resourceGroupName}, {workspaceName}, etc.).\n" +
            "2. **HTTP method** – e.g., GET, PUT, POST, DELETE, PATCH, etc.\n" +
            "3. **Request content** – show the request body only if required (in JSON format) show it in full, every supported property. If the operation doesn't require a request body, skip this field.\n\n" +
            "Ensure that all HTTP methods, including less common ones (like `POST` or `PATCH`), are extracted. Do not miss any resource. Return the result in a clean and readable Markdown format with clear labels for each operation.\n" +
            "Focus only on extracting the REST route, HTTP method, and request content (if applicable).\n\n" +
            "Make sure no resources are overlooked, including special operations like migrations, batch processes, and any other lesser-known operations. Focus on operations under the `paths` section of the specification." +
            "If the URL or HTML page is missing, prompt the user to provide a valid reference that depicts such a Swagger page or documentation." +
            "If the analysis is successful, and asked by the user return a list of all detected resources. The required information should be extracted and presented in accordance with the provided structured response format. ");

        // Initialize structured responses
        var restDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                restDiscoveryIsValid = new { type = "boolean", description = "Whether rest discovery was parsed" },
                instructionsToUser = new { type = "string", description = "Instructions for the user in case the rest discovery is not included or successfully concluded." },
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
            required = new[] { "restDiscoveryIsValid", "instructionsToUser", "detectedResources" }
        };

        StructuredResponses.Add<RestDiscoveryOutput>(PromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey,
            JsonSerializer.Serialize(restDiscoveryReturnedOutputSchema));
    }
}
