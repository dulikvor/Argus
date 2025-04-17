using Argus.Contracts.OpenAI;

namespace ApiTestingAgent.StateMachine
{
    public class ApiTestsStepResult
    {
        public bool StepSuccess { get; set; }
        public IReadOnlyList<CoPilotChatResponseMessage> CoPilotChatResponseMessages { get; set; }
        public IList<FunctionResponse> FunctionResponses { get; set; }
        public IList<string> AuxiliaryMessages { get; set; }
    }
}
