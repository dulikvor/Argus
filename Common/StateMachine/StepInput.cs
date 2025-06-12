using Argus.Common.StateMachine;
using Argus.Contracts.OpenAI;

namespace Argus.Common.StateMachine
{
    public class StepInput
    {
        public CoPilotChatRequestMessage CoPilotChatRequestMessage { get; set; }
        public StepResult PreviousStepResult { get; set; }

        public void ResetRequestMessage()
        {
            CoPilotChatRequestMessage = CoPilotChatRequestMessage.GetUserLast();
        }
    }
}
