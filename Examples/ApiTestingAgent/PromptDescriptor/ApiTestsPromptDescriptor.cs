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
            "### SYSTEM OVERVIEW\n" +
            "This state machine guides the user through a multi-step, LLM-driven API testing workflow. Each state is instrumented with telemetry (tracing, metrics, logging) and uses structured output schemas for intent detection, confirmation, and transitions. The workflow includes the following states and transitions:\n" +
            "1. **ServiceInformationState**: Discovers service and domain information. Transitions: set up service info, retry, or proceed to REST API discovery. Uses LLM prompts for extraction and confirmation.\n" +
            "2. **RestDiscoveryState**: Discovers REST API endpoints and details. Transitions: rediscover endpoints, fetch raw content, retry, or proceed to command discovery. Semantic context and LLM intent detection are used.\n" +
            "3. **CommandDiscoveryState**: Discovers available API commands/operations. Transitions: rediscover commands, retry, or proceed to expected outcome definition. LLM output is confirmed with user consent if needed.\n" +
            "4. **ExpectedOutcomeState**: Defines the expected outcome for a command (status, errors, response). Transitions: refine outcome, retry, or proceed to command invocation. Uses structured schemas for output and confirmation.\n" +
            "5. **CommandInvocationState**: Invokes the confirmed command and captures results. Transitions: retry command discovery, handle errors, or proceed to the final state. LLM output and telemetry are used for result analysis.\n" +
            "6. **EndState**: Marks successful completion or early termination of the workflow.\n" +
            "\nAt each step, the state machine leverages semantic memory, LLM-driven intent detection, and user consent/confirmation to ensure safe and accurate transitions. Error handling, retries, and fallback transitions are supported throughout the workflow.");
    }
}