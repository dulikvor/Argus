using OpenAI.Chat;
using System.Text.Json;

namespace Argus.Common.Functions
{
    public class FunctionDescriptor : IFunctionDescriptor
    {
        public ChatTool ToolDefinition { get => _toolDefinition; }
        protected ChatTool _toolDefinition;

        public string Name { get; }

        public TParameter GetParameters<TParameter>(string parameters)
        {
            return JsonSerializer.Deserialize<TParameter>(parameters);
        }

        protected FunctionDescriptor(string name)
        {
            Name = name;
        }
    }

    public class ConcreteFunctionDescriptor<TReturn, T1> : FunctionDescriptor
    {
        public delegate TReturn FunctionType(T1 arg1);

        public FunctionType Function
        {
            get => _function;
        }
        protected FunctionType _function;

        protected ConcreteFunctionDescriptor(string name)
            : base(name)
        {
        }
    }

    public class ConcreteFunctionDescriptor<TReturn, T1, T2> : FunctionDescriptor
    {
        public delegate TReturn FunctionType(T1 arg1, T2 arg2);

        public FunctionType Function
        {
            get => _function;
        }
        protected FunctionType _function;

        protected ConcreteFunctionDescriptor(string name)
            : base(name)
        {
        }
    }

    public class ConcreteFunctionDescriptor<TReturn, T1, T2, T3> : FunctionDescriptor
    {
        public delegate TReturn FunctionType(T1 arg1, T2 arg2, T3 arg3);

        public FunctionType Function
        {
            get => _function;
        }
        protected FunctionType _function;

        protected ConcreteFunctionDescriptor(string name)
            : base(name)
        {
        }
    }

    public class ConcreteFunctionDescriptor<TReturn, T1, T2, T3, T4> : FunctionDescriptor
    {
        public delegate TReturn FunctionType(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        public FunctionType Function
        {
            get => _function;
        }
        protected FunctionType _function;

        protected ConcreteFunctionDescriptor(string name)
            : base(name)
        {
        }
    }
}
