using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

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
}