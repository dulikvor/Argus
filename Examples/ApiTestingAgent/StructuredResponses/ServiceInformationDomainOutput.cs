using Argus.Common.StructuredResponses;
using Argus.Contracts.OpenAI;
using System.Text.Json.Serialization;
using static ApiTestingAgent.PromptDescriptor.PromptsConstants;

namespace ApiTestingAgent.StructuredResponses;
public class ServiceInformationDomainOutput
{
    [JsonPropertyName("serviceDomain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ServiceDomain { get; set; }

    [JsonPropertyName("stepIsConcluded")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool StepIsConcluded { get; set; }

    [JsonPropertyName("instructionsToUser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUser { get; set; }

    [JsonPropertyName("serviceDomainDetectedInCurrentIteration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ServiceDomainDetectedInCurrentIteration { get; set; }

    public override string ToString()
    {
        var prefix = StepIsConcluded ? CopilotChatIcons.Checkmark : string.Empty;
        var summary = StepIsConcluded ? $"\nSummary: Known domain is {ServiceDomain}" : string.Empty;
        return $"{prefix} {InstructionsToUser} {summary}\n\n";
    }
}