using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class ExpectedOutcomeOutput : BaseOutput
{
    [JsonPropertyName("isExpectedDetected")]
    public bool IsExpectedDetected { get; set; }

    [JsonPropertyName("expectedHttpStatusCode")]
    public int ExpectedHttpStatusCode { get; set; }

    [JsonPropertyName("expectedErrorMessage")]
    public string ExpectedErrorMessage { get; set; }

    [JsonPropertyName("expectedResponseContent")]
    public string ExpectedResponseContent { get; set; }

    [JsonPropertyName("instructionsToUserOnDetectedCommand")]
    public string InstructionsToUserOnDetectedCommand { get; set; }

    public override string ToString()
    {
        return $"Expected HTTP Status Code: {ExpectedHttpStatusCode}, " +
               $"Expected Error Message: {ExpectedErrorMessage}, " +
               $"Expected Response Content: {ExpectedResponseContent}";
    }

    public override string OutputIncrementalResult()
    {
        return ToString();
    }

    public override string InstructionsToUserOnDetected()
    {
        return InstructionsToUserOnDetectedCommand;
    }
}