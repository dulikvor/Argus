using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandInvocationOutput : BaseOutput
{
    [JsonPropertyName("stepIsConcluded")]
    public bool StepIsConcluded { get; set; }

    [JsonPropertyName("analysis")]
    public string Analysis { get; set; }

    [JsonPropertyName("actualResponse")]
    public object ActualResponse { get; set; }

    [JsonPropertyName("expectedOutcome")]
    public object ExpectedOutcome { get; set; }

    public override string ToString()
    {
        return $"Analysis: {Analysis}\n\nExpected Outcome: {ExpectedOutcome}\n\nActual Response: {ActualResponse}";
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