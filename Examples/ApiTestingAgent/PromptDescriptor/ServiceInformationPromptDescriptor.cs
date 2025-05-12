using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class ServiceInformationPromptDescriptor : BaseStatePromptDescriptor
{
    public override string DescriptorType => nameof(ServiceInformationPromptDescriptor);

    public ServiceInformationPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts[PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainPromptKey] =
        "Extract the user service domain. If missing, instruct the user to provide the domain. " +
        "For example, a service domain usually looks like 'example.com'. If the domain is already known, present it to the user for confirmation. " +
        "The operation cannot be completed until the user explicitly approves the domain. If the user wishes to provide a different domain, allow them to do so, but ensure explicit approval before proceeding.";

        // Initialize structured responses
        var serviceInformationDomainReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                serviceDomain = new { type = "string", description = "The service domain" },
                stepIsConcluded = new { type = "boolean", description = "Whether the step was concluded. This will be explicitly false until the customer approves the domain." },
                instructionsToUser = new
                {
                    type = "string",
                    description = "Instructions to the user in case service domain is not included or successfully concluded. If a known domain exists, it will be presented along with instructions on what is required now."
                },
                serviceDomainDetectedInCurrentIteration = new { type = "boolean", description = "True if a service domain was detected in the current iteration." }
            },
            required = new[] { "serviceDomain", "stepIsConcluded", "instructionsToUser", "serviceDomainDetectedInCurrentIteration" }
        };
        StructuredResponses.Add<ServiceInformationDomainOutput>(PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
            JsonSerializer.Serialize(serviceInformationDomainReturnedOutputSchema));
    }
}
