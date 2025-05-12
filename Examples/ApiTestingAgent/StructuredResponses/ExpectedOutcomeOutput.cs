using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class ExpectedOutcomeOutput : BaseOutput
{
    [JsonPropertyName("stepIsConcluded")]
    public bool StepIsConcluded { get; set; }

    [JsonPropertyName("expectedHttpStatusCode")]
    public int? ExpectedHttpStatusCode { get; set; }

    [JsonPropertyName("expectedErrorMessage")]
    public string ExpectedErrorMessage { get; set; }

    [JsonPropertyName("expectedResponseContent")]
    public string ExpectedResponseContent { get; set; }

    public override string ToString()
    {
        return $"Expected HTTP Status Code: {ExpectedHttpStatusCode}, " +
               $"Expected Error Message: {ExpectedErrorMessage}, " +
               $"Expected Response Content: {ExpectedResponseContent}";
    }

    public override string OutputIncrementalResult()
    {
        return $"Incremental Result: {ToString()}";
    }

    public override string OutputResult()
    {
        return $"Final Result: {ToString()}";
    }
}