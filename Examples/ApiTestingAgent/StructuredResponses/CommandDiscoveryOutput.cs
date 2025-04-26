using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandDiscoveryOutput
{
    [JsonPropertyName("commandIsValid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CommandIsValid { get; set; }

    [JsonPropertyName("selectedCommand")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SelectedCommand { get; set; }

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

    public override string ToString()
    {
        var prefix = CommandIsValid ? "✅" : string.Empty;
        var formattedMessage = $"{prefix} {InstructionsToUser}\n\n";

        formattedMessage += "**HTTP Method**: \"" + HttpMethod + "\"\n\n";
        formattedMessage += "**Request URI**: \"" + RequestUri + "\"\n\n";

        if (!string.IsNullOrWhiteSpace(Content))
        {
            formattedMessage += "**Request Content**:\n```json\n";
            formattedMessage += Content;
            formattedMessage += "\n```\n";
        }

        return formattedMessage;
    }
}