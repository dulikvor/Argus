using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class RestDiscoveryOutput
{
    [JsonPropertyName("restDiscoveryIsValid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool RestDiscoveryIsValid { get; set; }

    [JsonPropertyName("instructionsToUser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string InstructionsToUser { get; set; }

    [JsonPropertyName("detectedResources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<DetectedResource> DetectedResources { get; set; } = new();

    public override string ToString()
    {
        if (!RestDiscoveryIsValid)
        {
            return $"{InstructionsToUser}\n\n";
        }

        var formattedMessage = $"{InstructionsToUser}\n\n";
        for (int i = 0; i < DetectedResources.Count; i++)
        {
            var resource = DetectedResources[i];
            formattedMessage += $"\n\n### {i + 1}. **Rest route**: `{resource.ResourceDepiction}`\n\n";
            formattedMessage += $"**HTTP method**: `{resource.HttpMethod}`\n\n";

            if (!string.IsNullOrWhiteSpace(resource.SupportedJsonContent))
            {
                formattedMessage += "**Request content**:\n```json\n";
                formattedMessage += resource.SupportedJsonContent;
                formattedMessage += "\n```\n\n";
            }
        }
        return formattedMessage;
    }
}

public class DetectedResource
{
    [JsonPropertyName("resourceDepiction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ResourceDepiction { get; set; }

    [JsonPropertyName("httpMethod")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string HttpMethod { get; set; }

    [JsonPropertyName("supportedJsonContent")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string SupportedJsonContent { get; set; }

    public override string ToString()
    {
        return $"ResourceDepiction: {ResourceDepiction}, HttpMethod: {HttpMethod}, SupportedJsonContent: {SupportedJsonContent}";
    }
}
