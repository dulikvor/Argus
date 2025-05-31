namespace Argus.Common.PromptDescriptors
{
    public interface IPromptDescriptorFactory
    {
        IPromptDescriptor GetPromptDescriptor(string handlerType);
    }
}