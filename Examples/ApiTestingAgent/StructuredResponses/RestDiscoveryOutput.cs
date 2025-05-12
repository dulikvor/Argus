using System.Text.Json.Serialization;
using static ApiTestingAgent.PromptDescriptor.PromptsConstants;

namespace ApiTestingAgent.StructuredResponses;

public class RestDiscoveryOutput : BaseOutput
{
    [JsonPropertyName("restDiscoveryDetectedInCurrentIteration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool RestDiscoveryDetectedInCurrentIteration { get; set; }

    [JsonPropertyName("instructionsToUser")]
    public string InstructionsToUser { get; set; }

    [JsonPropertyName("detectedResources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DetectedResources DetectedResources { get; set; } = new();

    [JsonPropertyName("stepIsConcluded")]
    public bool StepIsConcluded { get; set; }

    public override string ToString()
    {
        var formattedMessage = string.Empty;
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

    public override string OutputIncrementalResult()
    {
        return $"Incremental Result: {ToString()}";
    }

    public override string OutputResult()
    {
        var formattedMessage = $"{InstructionsToUser}\n\n";
        formattedMessage += ToString();
        return formattedMessage;
    }
}

public class DetectedResources : List<DetectedResource>
{
    public override string ToString()
    {
        return string.Join(',', this.Select(x => x.ToString()));
    }

    public void MergeOrUpdate(IEnumerable<DetectedResource> newResources)
    {
        foreach (var newResource in newResources)
        {
            var existingResource = this.FirstOrDefault(r => r.HttpMethod.Equals(newResource.HttpMethod, StringComparison.OrdinalIgnoreCase) 
                && r.ResourceDepiction.Equals(newResource.ResourceDepiction, StringComparison.OrdinalIgnoreCase));

            if (existingResource != null)
            {
                // Update the existing resource if needed
                existingResource.SupportedJsonContent = newResource.SupportedJsonContent;
            }
            else
            {
                // Add the new resource
                this.Add(newResource);
            }
        }
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
