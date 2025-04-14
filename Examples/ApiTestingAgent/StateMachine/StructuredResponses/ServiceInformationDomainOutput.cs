using Argus.Common.StructuredResponses;
using Argus.Contracts.OpenAI;
using System.Text.Json.Serialization;
using static ApiTestingAgent.StateMachine.StatePromptsConstants;

namespace ApiTestingAgent.StateMachine.StructuredResponses;
public class ServiceInformationDomainOutput
{
    [JsonPropertyName("serviceDomain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ServiceDomain { get; set; }

    [JsonPropertyName("serviceDomainIsValid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ServiceDomainIsValid { get; set; }

    [JsonPropertyName("instructionsToUser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUser { get; set; }

    public override string ToString()
    {
        var prefix = ServiceDomainIsValid ? CopilotChatIcons.Checkmark : string.Empty;
        return $"{prefix} {InstructionsToUser}\n\n";
    }
}