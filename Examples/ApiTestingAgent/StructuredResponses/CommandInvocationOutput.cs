using Argus.Common.StructuredResponses;
using System.Text.Json.Serialization;

namespace ApiTestingAgent.StructuredResponses;

public class CommandInvocationAnalysisOutput : BaseOutput
{
    [JsonPropertyName("analysis")]
    public string Analysis { get; set; }

    [JsonPropertyName("outcomeMatched")]
    public bool OutcomeMatched { get; set; }

    [JsonPropertyName("correctedUserMessage")]
    public string CorrectedUserMessage { get; set; }

    public override string OutputIncrementalResult()
    {
        return Analysis;
    }

    public override string InstructionsToUserOnDetected()
    {
        return Analysis;
    }
}