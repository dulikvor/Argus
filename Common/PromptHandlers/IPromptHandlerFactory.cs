namespace Argus.Common.PromptHandlers
{
    public interface IPromptHandlerFactory
    {
        IStatePromptHandler GetPromptHandler(string handlerType);
    }
}