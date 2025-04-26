using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI;
public class CopilotConfirmationRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "action"; // Default to "action".

    [JsonPropertyName("title")]
    public string Title { get; set; } // Title of the confirmation dialog.

    [JsonPropertyName("message")]
    public string Message { get; set; } // Confirmation message shown to the user.

    [JsonPropertyName("confirmation")]
    public ConfirmationData Confirmation { get; set; } // Additional data for unique identification.
}