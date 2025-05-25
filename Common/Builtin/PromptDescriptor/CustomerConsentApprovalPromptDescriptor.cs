using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.PromptDescriptors;
using System.Text.Json;

namespace Argus.Common.Builtin.PromptDescriptor
{
    public class CustomerConsentStateTransitionPromptDescriptor : BasePromptDescriptor
    {
        public override string DescriptorType => nameof(CustomerConsentStateTransitionPromptDescriptor);

        public CustomerConsentStateTransitionPromptDescriptor()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Initialize prompts
            Prompts[PromptsConstants.Prompts.Keys.CustomerConsentStateTransition] = "You are tasked with detecting whether a customer consent approval is being requested and determining the user's stance on it.\n" +
            "\n" +
            "Your output must be a JSON object with the following structure:\n" +
            "{\n" +
            "  \"action\": \"ConsentApproval\" | \"Unknown\",\n" +
            "  \"approvalStatus\": \"Approved\" | \"Disapproved\" | null,\n" +
            "  \"reasoning\": \"<string explaining your classification>\"\n" +
            "}\n" +
            "\n" +
            "Guidelines:\n" +
            "- If the input refers to a consent approval, set action = 'ConsentApproval'.\n" +
            "- If the user agrees or accepts, set approvalStatus = 'Approved'.\n" +
            "- If the user disagrees or rejects, set approvalStatus = 'Disapproved'.\n" +
            "- If the input is unrelated to consent, set action = 'Unknown' and approvalStatus = null.\n" +
            "- Always explain your reasoning in the 'reasoning' field, and make it specific to the input.";

            // Initialize structured responses
            var customerConsentApprovalSchema = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", description = "Indicates the action type: ConsentApproval or Unknown." },
                    approvalStatus = new { type = "string", description = "Indicates the approval status: Approved or Disapproved." },
                    reasoning = new { type = "string", description = "Explanation for the decision made by the LLM." }
                },
                required = new[] { "action", "approvalStatus", "reasoning" }
            };

            StructuredResponses.Add<CustomerConsentStateTransitionResponse>(PromptsConstants.StructuredResponses.Keys.CustomerConsentStateTransitionResponseSchema,
                JsonSerializer.Serialize(customerConsentApprovalSchema));
        }
    }
}
