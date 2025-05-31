using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;
public class ServiceInformationDomainOutput : BaseOutput
{
    [JsonPropertyName("detectedDomain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string DetectedDomain { get; set; }

    [JsonPropertyName("instructionsToUserOnDomainDetected")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUserOnDomainDetected { get; set; }

    [JsonPropertyName("stepIsConcluded")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool StepIsConcluded { get; set; }

    [JsonPropertyName("instructionsToUserOnFailure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUserOnFailure { get; set; }

    [JsonPropertyName("serviceDomainDetectedInCurrentIteration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ServiceDomainDetectedInCurrentIteration { get; set; }

    public override string InstructionsToUserOnDetected()
    {
        return InstructionsToUserOnDomainDetected;
    }

    public override string OutputIncrementalResult()
    {
        return $"Detected Domain: {DetectedDomain}";
    }
}