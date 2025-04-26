using System.Text.Json.Serialization;

namespace Argus.Contracts.OpenAI;

public class ConfirmationData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } // Unique identifier for the confirmation.

    [JsonPropertyName("other")]
    public string Other { get; set; } // Optional additional identifier.
}