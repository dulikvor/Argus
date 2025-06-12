using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using Argus.Common.StateMachine;
using Argus.Common.Swagger;

namespace ApiTestingAgent.Services;

public class ApiTestSession : Session<ApiTestStateTransitions, StepInput>
{
    public List<SwaggerOperation> Resources
    {
        get
        {
            var compiledKey = CompileKey(nameof(RestDiscoveryState), PromptsConstants.SessionResult.Keys.DetectedResourcesKey);
            if (StepResult.TryGetValue(compiledKey, out var value) && value is List<SwaggerOperation> resources)
            {
                return resources;
            }

            var newResources = new List<SwaggerOperation>();
            return newResources;
        }
    }

    public bool ResourcesExists() => Resources.Any();

    public void MergeOrAddResource(ref List<SwaggerOperation> resources, SwaggerOperation operation)
    {
        var existing = resources.Find(r => r.Url == operation.Url && r.HttpMethod.Equals(operation.HttpMethod, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            // Replace content with the new one
            existing.Content = operation.Content;
        }
        else
        {
            resources.Add(operation);
        }
    }

    public void MergeOrAddResources(IEnumerable<SwaggerOperation> operations)
    {
        var resources = Resources;
        foreach (var op in operations)
        {
            MergeOrAddResource(ref resources, op);
        }

        if (resources?.Any() == true)
        {
            StepResult[CompileKey(nameof(RestDiscoveryState), PromptsConstants.SessionResult.Keys.DetectedResourcesKey)] = resources;
        }
    }

    public ApiTestSession() : base()
    {
    }
}