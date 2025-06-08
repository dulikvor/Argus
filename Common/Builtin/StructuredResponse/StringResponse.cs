using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace Argus.Common.Builtin.StructuredResponse
{
    public class StringResponse : BaseOutput
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } // Explanation for the decision made by the LLM

        public override string InstructionsToUserOnDetected()
        {
            return Content;
        }
        public override string OutputIncrementalResult()
        {
            throw new NotImplementedException("StringOutput does not support incremental results.");
        }
    }
}
