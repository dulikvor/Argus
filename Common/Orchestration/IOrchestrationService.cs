using Argus.Common.Builtin.StructuredResponse;
using Argus.Common.StateMachine;
using Argus.Common.StructuredResponses;
using Argus.Contracts.OpenAI;
using OpenAI.Chat;

namespace Argus.Common.Orchestration
{
    public interface IOrchestrationService
        <TTransition, TStepInput>
        where TTransition : Enum
        where TStepInput : StepInput
    {
        Task<(bool IsConsentGiven, ConsentAction Action, ChatCompletion ChatCompletion)> CheckCustomerConsent(Session<TTransition, TStepInput> session, TStepInput stepInput);
        Task<(StepResult, TTransition)> DetectAndConfirm<TOutput>(Session<TTransition, TStepInput> session, TStepInput stepInput, (string, string) stepResultKey, ChatCompletionStructuredResponse<TOutput> chatCompletionStructuredResponse, Func<TOutput, bool> isDetected, Func<TOutput, string> getConfirmationMessage, TTransition retryTransition, bool withSoftConsent = false, bool withStepResult = true) where TOutput : BaseOutput;
        Task<ChatCompletionStructuredResponse<TOutput>> QueryLLM<TOutput>(CoPilotChatRequestMessage requestMessage, string promptDescriptorName, string promptKey, string outputKey, List<ChatTool> tools, bool withContext = true) where TOutput : BaseOutput;
    }
}