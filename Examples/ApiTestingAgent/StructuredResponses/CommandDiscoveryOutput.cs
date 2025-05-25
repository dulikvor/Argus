using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandDiscoveryOutput : BaseOutput
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

    public override string ToString()
    {
        var formattedMessage = "**HTTP Method**: \"" + HttpMethod + "\"\n\n";
        formattedMessage += "**Request URI**: \"" + RequestUri + "\"\n\n";

        if (!string.IsNullOrWhiteSpace(Content))
        {
            formattedMessage += "**Request Content**:\n```json\n";
            formattedMessage += Content;
            formattedMessage += "\n```\n";
        }

        return formattedMessage;
    }

    public override string InstructionsToUserOnDetected()
    {
        return InstructionsToUser;
    }

    public override string OutputIncrementalResult()
    {
        return ToString();
    }
}