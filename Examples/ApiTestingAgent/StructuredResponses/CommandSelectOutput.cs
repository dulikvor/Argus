using Argus.Common.StructuredResponses;
using System.Text;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandSelectOutput : BaseOutput
{
    [JsonPropertyName("commandIsValid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CommandIsValid { get; set; }

    [JsonPropertyName("instructionsToUser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUser { get; set; }

    [JsonPropertyName("httpMethod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string HttpMethod { get; set; }

    [JsonPropertyName("requestUri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string RequestUri { get; set; }

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Content { get; set; }

    [JsonPropertyName("commandDiscoveryDetectedInCurrentIteration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CommandDiscoveryDetectedInCurrentIteration { get; set; } // Indicates if a change due to user request was detected, such as updates to the selected command method, URI, or placeholders.

    public override string InstructionsToUserOnDetected()
    {
        return InstructionsToUser;
    }

    public override string OutputIncrementalResult()
    {
        var sb = new StringBuilder();
        sb.Append($"Selected Command:\n");
        sb.Append($"Http Method: {HttpMethod}\n");
        sb.Append($"Request Uri: {RequestUri}\n");
        sb.Append($"Request Content:```json\n{Content}\n```\n");
        sb.AppendLine();

        return sb.ToString();
    }
}