using Argus.Common.PromptDescriptors;
using Argus.Common.Swagger;
using System.Text;

namespace ApiTestingAgent.PromptDescriptor;
public class ApiTestsSessionPromptDescriptor : BasePromptDescriptor
{
    public override string DescriptorType => nameof(ApiTestsSessionPromptDescriptor);

    public ApiTestsSessionPromptDescriptor()
    {
        Initialize();
    }

    protected override void Initialize()
    {
        Prompts.Add(PromptsConstants.ApiTests.Keys.DetectedOperationsKey,
            "Detected Commands With Content:\r\n\r\n\"A list of known REST API operations discovered earlier, including their HTTP method, request URI, and full JSON payload structure.\"");
    }

    public string ReconcileDetectedOperationContextPrompt(List<SwaggerOperation> operations)
    {
        var sb = new StringBuilder();

        var prompt = GetPrompt(PromptsConstants.ApiTests.Keys.DetectedOperationsKey);
        sb.AppendLine(prompt);
        foreach (var operation in operations)
        {
            sb.Append($"Http Method: {operation.HttpMethod}\n");
            sb.Append($"Request Uri: {operation.Url}\n");
            sb.Append($"Request Content:```json\n{operation.Content}\n```\n");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}