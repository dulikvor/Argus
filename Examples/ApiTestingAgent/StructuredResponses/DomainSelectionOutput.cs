using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;
public class DomainSelectionOutput : BaseOutput
{
    [JsonPropertyName("detectedDomain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string DetectedDomain { get; set; }

    [JsonPropertyName("userResponse")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string UserResponse { get; set; }

    public override string InstructionsToUserOnDetected()
    {
        return UserResponse;
    }

    public override string OutputIncrementalResult()
    {
        return $"Detected Domain: {DetectedDomain}\n";
    }
}