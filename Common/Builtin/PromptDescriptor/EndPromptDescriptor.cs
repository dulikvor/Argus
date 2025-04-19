using Argus.Common.PromptDescriptors;

namespace Argus.Common.Builtin.PromptDescriptor;
public class EndPromptDescriptor : BaseStatePromptDescriptor
{
    public override string DescriptorType => nameof(EndPromptDescriptor);

    public EndPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        // Initialize prompts
        Prompts.Add(PromptsConstants.Keys.EndState,
            "This is the final state. The current session is concluded. " +
            "Do you want to retry or reset any operation?"
           );
    }
}