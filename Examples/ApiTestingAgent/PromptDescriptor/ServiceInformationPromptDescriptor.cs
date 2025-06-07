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
            @"### SERVICE DOMAIN SELECTION
            #### 🎯 Goal:
            Identify the service domain to be used for command construction in a later step.

            ---

            #### 📌 Current Domain:
            {0}

            ---

            #### 💬 What You Can Do:
            - ✅ If the domain shown above is correct, confirm by replying with “yes”, “confirm”, “use this domain”, or any similar confirmation.
            - 🔁 If you'd like to use a different domain, please enter it now.
            - 🆕 If no domain is currently shown, you must enter one to continue.

            ---

            #### 📤 Response Format:
            Respond in the following structured format:

            ```json
            {
              ""domain"": ""<string>"",        // The domain to be used for command construction.
              ""userResponse"": ""<string>""   // The natural-language confirmation or message to the user.
            }
            ";

        // Initialize structured responses
        var serviceInformationDomainReturnedOutputSchema = new
        {
            type = "object",
            properties = new
            {
                detectedDomain = new
                {
                    type = "string",
                    description = "The domain provided or confirmed by the user in the current interaction, such as 'api.example.com'. This must be null if no domain was detected in this input."
                },
                userResponse = new
                {
                    type = "string",
                    description = "The message shown to the user, clearly acknowledging the detected domain or guiding the user to provide one."
                }
            },
            required = new[] { "detectedDomain", "userResponse" }
        };
        StructuredResponses.Add<DomainSelectionOutput>(PromptsConstants.ServiceInformation.Keys.ServiceInformationDomainReturnedOutputKey,
            JsonSerializer.Serialize(serviceInformationDomainReturnedOutputSchema));
    }
}
