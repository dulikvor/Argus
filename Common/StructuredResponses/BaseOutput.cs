namespace Argus.Common.StructuredResponses;

public abstract class BaseOutput
{
    public abstract string InstructionsToUserOnDetected();
    public abstract string OutputIncrementalResult();
}