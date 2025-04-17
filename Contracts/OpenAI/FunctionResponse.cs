namespace Argus.Contracts.OpenAI
{
    public class FunctionResponse
    {
        public string FunctionName { get; set; }
        public Dictionary<string, object> FunctionArguments { get; set; }
    }
}
