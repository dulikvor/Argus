namespace Argus.Common.PromptDescriptors
{
    public class PromptDescriptorFactory : IPromptDescriptorFactory
    {
        private readonly Dictionary<string, IPromptDescriptor> PromptDescriptors = new();

        public PromptDescriptorFactory(IEnumerable<IPromptDescriptor> promptDescriptors)
        {
            foreach (var descriptor in promptDescriptors)
            {
                if (descriptor is IPromptDescriptor statePromptDescriptor)
                {
                    PromptDescriptors[statePromptDescriptor.DescriptorType] = statePromptDescriptor;
                }
            }
        }

        public IPromptDescriptor GetPromptDescriptor(string descriptorType)
        {
            if (PromptDescriptors.TryGetValue(descriptorType, out var descriptor))
            {
                return descriptor;
            }

            throw new ArgumentException($"Unknown handler type: {descriptorType}", nameof(descriptorType));
        }
    }
}