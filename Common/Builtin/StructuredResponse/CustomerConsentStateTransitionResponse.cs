using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace Argus.Common.Builtin.StructuredResponse
{
    public class CustomerConsentStateTransitionResponse : BaseOutput
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("action")]
        public ConsentAction Action { get; set; } // Enum for ConsentApproval or Unknown

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("approvalStatus")]
        public ApprovalStatus? ApprovalStatus { get; set; } // Enum for Approved or Disapproved

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; } // Explanation for the decision made by the LLM

        public override string InstructionsToUserOnDetected()
        {
            throw new NotImplementedException();
        }

        public override string OutputIncrementalResult()
        {
            throw new NotImplementedException();
        }
    }
}
