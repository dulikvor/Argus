using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses
{
    public class CommandInvocationDetectNextStateOutput : BaseOutput
    {
        [JsonPropertyName("nextState")]
        public string NextState { get; set; }

        [JsonPropertyName("currentStatus")]
        public string CurrentStatus { get; set; }

        [JsonPropertyName("reasoning")]
        public string Reasoning { get; set; }

        public override string OutputIncrementalResult()
        {
            return CurrentStatus;
        }

        public override string InstructionsToUserOnDetected()
        {
            return CurrentStatus;
        }
    }
}
