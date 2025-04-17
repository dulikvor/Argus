using Argus.Common.StructuredResponses;

namespace Argus.Common.PromptDescriptors;

public abstract class BaseStatePromptDescriptor : IStatePromptDescriptor
{
    protected readonly Dictionary<string, string> Prompts = new();
    protected readonly StructuredResponsesDictionary StructuredResponses = new();

    public string GetPrompt(string key)
    {
        if (Prompts.TryGetValue(key, out var prompt))
        {
            return prompt;
        }
        throw new KeyNotFoundException($"Prompt with key '{key}' not found.");
    }

    public string GetStructuredResponse(string key)
    {
        if (StructuredResponses.TryGetValue(key, out var response))
        {
            return response;
        }
        throw new KeyNotFoundException($"Structured response with key '{key}' not found.");
    }

    protected abstract void Initialize();

    public abstract string DescriptorType { get; }
}
