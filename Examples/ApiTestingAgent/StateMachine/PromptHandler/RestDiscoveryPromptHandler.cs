using ApiTestingAgent.StateMachine.StructuredResponses;
using Argus.Common.PromptHandlers;
using System.Text.Json;

namespace ApiTestingAgent.StateMachine.PromptHandlers;
public class RestDiscoveryPromptHandler : BaseStatePromptHandler
{
    public override string HandlerType => nameof(RestDiscoveryPromptHandler);

    public RestDiscoveryPromptHandler()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryPromptKey,
            "Extract the user REST resources and their child resources. A reference to these resources is expected to be provided in a URL that the LLM should analyze. " +
            "The reference should ideally point to a Swagger-formatted web page. If the URL is missing, prompt the user to provide a valid URL that depicts such a Swagger page. " +
            "If the analysis is successful, return a list of all detected resources. The required information should be extracted and presented in accordance with the provided structured response format. ");

        // Initialize structured responses
        var restDiscoveryReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                serviceDomain = new { type = "string", description = "The service domain" },
                restDiscoveryIsValid = new { type = "boolean", description = "Whether rest discovery was parsed" },
                instructionsToUser = new { type = "string", description = "Instructions for the user in case the service domain is not included or successfully concluded." },
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
                },
                required = new[] { "serviceDomain", "restDiscoveryIsValid", "instructionsToUser", "detectedResources" }
            }
        };

        StructuredResponses.Add<ServiceInformationDomainOutput>(StatePromptsConstants.RestDiscovery.Keys.RestResourcesDiscoveryReturnedOutputKey,
            JsonSerializer.Serialize(restDiscoveryReturnedOutputSchema));
    }
}
