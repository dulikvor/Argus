namespace Argus.Common.PromptDescriptors;

public interface IStatePromptDescriptor
{
    public string GetPrompt(string key);
    public string GetStructuredResponse(string key);
    public string DescriptorType { get; }
}