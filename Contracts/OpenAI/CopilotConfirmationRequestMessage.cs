using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI;
public class CopilotConfirmationRequestMessage
{
    [JsonPropertyName("type")]
    public string Type { get; } = "action"; // Default to "action".

    [JsonPropertyName("title")]
    public string Title { get; set; } // Title of the confirmation dialog.

    [JsonPropertyName("message")]
    public string Message { get; set; } // Confirmation message shown to the user.

    [JsonPropertyName("confirmation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ConfirmationData Confirmation { get; set; }

    public static ConfirmationData GenerateConfirmationData()
    {
        return new ConfirmationData
        {
            Id = $"id-{Guid.NewGuid().ToString()}"
        };
    }
}
