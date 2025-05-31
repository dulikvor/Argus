using Argus.Common.StructuredResponses;
using OpenAI.Chat;
using System.Text.Json;

namespace Argus.Contracts.OpenAI
{
    public class ChatCompletionStructuredResponse<TStructure>
        where TStructure : class
    {
        public TStructure StructuredOutput { get; }
        public IList<FunctionResponse> FunctionResponses { get; }
        public ChatCompletion ChatCompletion { get; }
        public bool IsToolCall { get; }

        public ChatCompletionStructuredResponse(ChatCompletion chatCompletion)
        {
            ChatCompletion = chatCompletion;

            if (chatCompletion.FinishReason == ChatFinishReason.Stop)
            {
                IsToolCall = false;

                if (typeof(TStructure) == typeof(string))
                {
                    StructuredOutput = chatCompletion.Content.First().Text as TStructure;
                }
                else
                {
                    StructuredOutput = JsonSerializer.Deserialize<TStructure>(chatCompletion.Content.First().Text);
                }
            }
            else if (chatCompletion.FinishReason == ChatFinishReason.ToolCalls)
            {
                IsToolCall = true;
                FunctionResponses = chatCompletion.ToolCalls.Select(tc => new FunctionResponse()
                {
                    FunctionName = tc.FunctionName,
                    FunctionArguments = JsonSerializer.Deserialize<Dictionary<string, object>>(tc.FunctionArguments.ToString())
                }).ToList();
            }
            else
            {
                throw new InvalidOperationException($"Chat completion completed with unsupported finish reason: {chatCompletion.FinishReason}");
            }
        }
    }
}
