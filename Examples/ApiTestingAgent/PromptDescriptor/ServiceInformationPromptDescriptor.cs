using ApiTestingAgent.StructuredResponses;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace ApiTestingAgent.PromptDescriptor;
public class ServiceInformationPromptDescriptor : BasePromptDescriptor
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
        "Detect if a service domain (like \"example.com\") is mentioned by the user. " +
        "If it is detected, return a full sentence in `instructionsToUserOnDomainDetected` confirming what was found and asking the user for approval or correction. " +
        "Set `stepIsConcluded = true` if the user confirms the domain � any approval is legit. " +
        "Once confirmed, `instructionsToUserOnDomainDetected` should contain a summary of the approved domain, and `detectedDomain` should include the detected domain value. " +
        "If no domain is found, ask the user to provide one, with a helpful instruction and an example. The instruction should be made over `instructionsToUserOnFailure`.";

        // Initialize structured responses
        var serviceInformationDomainReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                instructionsToUserOnDomainDetected = new { type = "string", description = "A full, user-friendly message shown when a service domain has been detected. It should clearly state the detected domain and instruct the user to either confirm it or provide a correction. Do not ask for approval in a vague way � make it clear that explicit confirmation is required before proceeding." },
                stepIsConcluded = new { type = "boolean", description = "True if the user has confirmed the domain � for example, by saying \"Yes, acme.org is correct\", \"That�s right\", \"Confirmed\", or similar phrasing." },
                instructionsToUserOnFailure = new
                {
                    type = "string",
                    description = "Message shown when no service domain is detected in the user input. It should clearly inform the user that a domain is required and provide an example (e.g., example.com) to guide them in supplying one."
                },
                serviceDomainDetectedInCurrentIteration = new { type = "boolean", description = "True if a valid service domain was detected in the current user input only (not from history). False if no new domain was found. This helps track whether detection happened in this specific interaction." },
                detectedDomain = new { type = "string", description = "The detected domain value, such as 'example.com', which was identified in the user input." }
            },
            required = new[] { "instructionsToUserOnDomainDetected", "stepIsConcluded", "instructionsToUserOnFailure", "serviceDomainDetectedInCurrentIteration", "detectedDomain" }
        };
        StructuredResponses.Add<ServiceInformationDomainOutput>(PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
            JsonSerializer.Serialize(serviceInformationDomainReturnedOutputSchema));
    }
}
