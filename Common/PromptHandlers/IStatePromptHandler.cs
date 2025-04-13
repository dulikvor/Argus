namespace Argus.Common.PromptHandlers;

public interface IStatePromptHandler
{
    public string GetPrompt(string key);
    public string GetStructuredResponse(string key);
    public string HandlerType { get; }
}