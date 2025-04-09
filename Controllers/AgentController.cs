using Argus.Clients.GitHubLLMQuery;
using Argus.Common.GitHubAuthentication;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Argus.Controllers;

[ApiController]
[Authorize]
[GitHubAuthenticationContextFilter]
public class AgentController : ControllerBase
{
    private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
    private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IGitHubLLMQueryClient gitHubLLMQueryClient,
        IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter,
        ILogger<AgentController> logger)
    {
        _gitHubLLMQueryClient = gitHubLLMQueryClient;
        _responseStreamWriter = responseStreamWriter;
        _logger = logger;
    }

    [HttpPost("/nextEvent")]
    public async Task NextEvent([FromBody] CoPilotChatRequestMessage coPilotChatRequestMessage)
    {
        var streamingChatCompletionUpdates = await _gitHubLLMQueryClient.Query(coPilotChatRequestMessage);

        HttpContext.Response.StatusCode = 200;
        await _responseStreamWriter.WriteToStreamAsync(HttpContext, streamingChatCompletionUpdates);
    }
}
