namespace Argus.Common.Functions
{
    public interface IFunctionDescriptorFactory
    {
        public IFunctionDescriptor GetFunctionDescriptor(string functionDescriptorName);
    }
}