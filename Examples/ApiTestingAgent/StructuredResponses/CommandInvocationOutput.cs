using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandInvocationOutput : BaseOutput
{
    [JsonPropertyName("isExpectedDetected")]
    public bool IsExpectedDetected { get; set; }

    [JsonPropertyName("analysis")]
    public string Analysis { get; set; }

    public override string OutputIncrementalResult()
    {
        return Analysis;
    }

    public override string InstructionsToUserOnDetected()
    {
        return Analysis;
    }
}