namespace Argus.Common.PromptHandlers
{
    public class PromptHandlerFactory : IPromptHandlerFactory
    {
        private readonly Dictionary<string, IStatePromptHandler> PromptHandlers = new();

        public PromptHandlerFactory(IEnumerable<IStatePromptHandler> promptHandlers)
        {
            foreach (var handler in promptHandlers)
            {
                if (handler is IStatePromptHandler statePromptHandler)
                {
                    PromptHandlers[statePromptHandler.HandlerType] = statePromptHandler;
                }
            }
        }

        public IStatePromptHandler GetPromptHandler(string handlerType)
        {
            if (PromptHandlers.TryGetValue(handlerType, out var handler))
            {
                return handler;
            }

            throw new ArgumentException($"Unknown handler type: {handlerType}", nameof(handlerType));
        }
    }
}