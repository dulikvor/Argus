using Argus.Common.PromptDescriptors;

namespace ApiTestingAgent.PromptDescriptor;
public class ApiTestsPromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(ApiTestsPromptDescriptor);

    public ApiTestsPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(PromptsConstants.ApiTests.Keys.StateMachineKey,
            "StateMachineOverview" + Environment.NewLine +
            "This state machine guides the user through a series of operations. " +
            "Currently, it includes the following steps:\n" +
            "1. **ServiceInformationState**: Handles the discovery of service information and domain details. " +
            "Transitions include setting up service information or moving to REST API discovery.\n" +
            "2. **RestDiscoveryState**: Handles the discovery of REST API endpoints and their details. " +
            "Transitions include rediscovering REST APIs, fetching raw content, or moving to command discovery.\n" +
            "3. **CommandDiscoveryState**: Handles the discovery of commands or operations related to the API. " +
            "Transitions include rediscovering commands or moving to define the expected outcome.\n" +
            "4. **ExpectedOutcomeState**: Defines the expected outcome of a command, including HTTP status, error messages, and response content. " +
            "Transitions include refining the expected outcome or moving to command invocation.\n" +
            "5. **CommandInvocationState**: Handles the invocation of a confirmed command. " +
            "Transitions include retrying command discovery or moving to the final state.\n" +
            "6. **EndState**: Represents the final state of the state machine, marking the successful completion of the workflow.");
    }
}