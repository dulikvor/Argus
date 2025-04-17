namespace Argus.Common.PromptDescriptors
{
    public class PromptDescriptorFactory : IPromptDescriptorFactory
    {
        private readonly Dictionary<string, IStatePromptDescriptor> PromptDescriptors = new();

        public PromptDescriptorFactory(IEnumerable<IStatePromptDescriptor> promptDescriptors)
        {
            foreach (var descriptor in promptDescriptors)
            {
                if (descriptor is IStatePromptDescriptor statePromptDescriptor)
                {
                    PromptDescriptors[statePromptDescriptor.DescriptorType] = statePromptDescriptor;
                }
            }
        }

        public IStatePromptDescriptor GetPromptDescriptor(string descriptorType)
        {
            if (PromptDescriptors.TryGetValue(descriptorType, out var descriptor))
            {
                return descriptor;
            }

            throw new ArgumentException($"Unknown handler type: {descriptorType}", nameof(descriptorType));
        }
    }
}