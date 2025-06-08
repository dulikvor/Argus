using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using ApiTestingAgent.StructuredResponses;
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

    public void MergeOrAddResource(SwaggerOperation operation)
    {
        var resources = Resources;
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
        foreach (var op in operations)
        {
            MergeOrAddResource(op);
        }

        if (Resources?.Any() == true)
        {
            StepResult[CompileKey(nameof(RestDiscoveryState), PromptsConstants.SessionResult.Keys.DetectedResourcesKey)] = Resources;
        }
    }

    public ApiTestSession() : base()
    {
    }
}