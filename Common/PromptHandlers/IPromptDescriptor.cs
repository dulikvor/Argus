namespace Argus.Common.PromptDescriptors;

public interface IPromptDescriptor
{
    public string GetPrompt(string key);
    public string GetStructuredResponse(string key);
    public string DescriptorType { get; }
}