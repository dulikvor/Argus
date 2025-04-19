using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Common.StateMachine
{
    public class StepResult
    {
        public bool StepSuccess { get; set; }
        public IReadOnlyList<CoPilotChatResponseMessage> CoPilotChatResponseMessages { get; set; }
        public IList<FunctionResponse> FunctionResponses { get; set; }
        public ChatCompletion PreviousChatCompletion { get; set; }
    }
}
