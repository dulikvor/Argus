namespace Argus.Common.Functions
{
    public class FunctionDescriptorFactory : IFunctionDescriptorFactory
    {
        private readonly Dictionary<string, IFunctionDescriptor> _functionDescriptors = new();

        public FunctionDescriptorFactory(IEnumerable<IFunctionDescriptor> functionDescriptors)
        {
            foreach (var functionDescriptor in functionDescriptors)
            {
                _functionDescriptors[functionDescriptor.Name] = functionDescriptor;
            }
        }

        public IFunctionDescriptor GetFunctionDescriptor(string functionDescriptorName)
        {
            if (_functionDescriptors.TryGetValue(functionDescriptorName, out var functionDescriptor))
            {
                return functionDescriptor;
            }

            throw new ArgumentException($"Unknown function descriptor type: {functionDescriptorName}", nameof(functionDescriptorName));
        }
    }
}