using OpenAI.Chat;

namespace Argus.Common.Functions
{
    public interface IFunctionDescriptor
    {
        public ChatTool ToolDefinition { get; }
        public string Name { get; }

        public TParameter GetParameters<TParameter>(string parameters);
    }
}
