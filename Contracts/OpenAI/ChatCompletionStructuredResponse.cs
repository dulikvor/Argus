using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Argus.Contracts.OpenAI
{
    public class ChatCompletionStructuredResponse<TStructure>
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
                StructuredOutput = JsonSerializer.Deserialize<TStructure>(chatCompletion.Content.First().Text);
            }
            else if(chatCompletion.FinishReason == ChatFinishReason.ToolCalls)
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
                throw new InvalidOperationException($"Chat completion completed with un supported finish reason: {chatCompletion.FinishReason}");
            }
        }
    }
}
