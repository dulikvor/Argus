using Argus.Common.PromptDescriptors;

namespace Argus.Common.Builtin.PromptDescriptor;
public class CurrentStatePromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(CurrentStatePromptDescriptor);

    public CurrentStatePromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(PromptsConstants.Prompts.Keys.CurrentState,
            "Welcome! You are currently in the \"{stateName}\" state. " +
            "Here is what you need to do: {stateInstructions}. " +
            "Please follow the instructions to proceed."
        );
    }
}
