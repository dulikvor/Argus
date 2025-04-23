using Argus.Common.PromptDescriptors;

namespace ApiTestingAgent.PromptDescriptor;
public class ApiTestsPromptDescriptor : BaseStatePromptDescriptor
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
            "1. **ServiceInformationDomain**: This step involves parsing the service domain provided by the user, validating its format, and determining if it is a valid domain. " +
            "If the domain is invalid or missing, the user is provided with specific instructions to correct the input. " +
            "2. **RestDiscovery**: It guides the user through identifying available REST API endpoints, understanding their structure, and gathering relevant metadata for further processing.");
    }
}