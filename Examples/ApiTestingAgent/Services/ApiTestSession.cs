using ApiTestingAgent.PromptDescriptor;
using ApiTestingAgent.StateMachine;
using ApiTestingAgent.StateMachine.Steps;
using ApiTestingAgent.StructuredResponses;
using Argus.Common.StateMachine;

namespace ApiTestingAgent.Services;

public class ApiTestSession : Session<ApiTestStateTransitions, StepInput>
{
    public DetectedResources DetectedResources
    {
        get
        {
            var compiledKey = CompileKey(nameof(RestDiscoveryState), PromptsConstants.SessionResult.Keys.DetectedResourcesKey);
            if (StepResult.TryGetValue(compiledKey, out var value) && value is DetectedResources resources)
            {
                return resources;
            }

            var newResources = new DetectedResources();
            StepResult[compiledKey] = newResources;
            return newResources;
        }
    }

    public ApiTestSession() : base()
    {
    }
}