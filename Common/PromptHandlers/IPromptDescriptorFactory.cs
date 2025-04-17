namespace Argus.Common.PromptDescriptors
{
    public interface IPromptDescriptorFactory
    {
        IStatePromptDescriptor GetPromptDescriptor(string handlerType);
    }
}