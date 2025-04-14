using ApiTestingAgent.StateMachine.StructuredResponses;
using Argus.Common.PromptHandlers;
using System.Text.Json;

namespace ApiTestingAgent.StateMachine.PromptHandlers;
public class ServiceInformationPromptHandler : BaseStatePromptHandler
{
    public override string HandlerType => nameof(ServiceInformationPromptHandler);

    public ServiceInformationPromptHandler()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey,
        "Extract the user service domain. If missing, instruct the user to provide the domain. " +
        "For example, a service domain usually looks like 'example.com'. If the domain is already known, return it to the user.");

        // Initialize structured responses
        var serviceInformationDomainReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                serviceDomain = new { type = "string", description = "The service domain" },
                serviceDomainIsValid = new { type = "boolean", description = "Whether service domain was parsed" },
                instructionsToUser = new { type = "string", description = "Instructions to the user in case service domain is not included or successfully concluded." }
            },
            required = new[] { "serviceDomain", "serviceDomainIsValid", "instructionsToUser" }
        };
        StructuredResponses.Add<ServiceInformationDomainOutput>(StatePromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
            JsonSerializer.Serialize(serviceInformationDomainReturnedOutputSchema));
    }
}
