using ApiTestingAgent.Services;
using Argus.Clients.GitHubLLMQuery;
using Argus.Common.GitHubAuthentication;
using Argus.Common.Web;
using Argus.Contracts.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiTestingAgent.Controllers;

[ApiController]
[Authorize]
[GitHubAuthenticationContextFilter]
public class AgentController : ControllerBase
{
    private readonly IApiTestService _apiTestService;
    private readonly IGitHubLLMQueryClient _gitHubLLMQueryClient;
    private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IApiTestService apiTestService,
        IGitHubLLMQueryClient gitHubLLMQueryClient,
        IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter,
        ILogger<AgentController> logger)
    {
        _apiTestService = apiTestService;
        _gitHubLLMQueryClient = gitHubLLMQueryClient;
        _responseStreamWriter = responseStreamWriter;
        _logger = logger;
    }

    [HttpPost("/nextEvent")]
    public async Task NextEvent([FromBody] CoPilotChatRequestMessage coPilotChatRequestMessage)
    {
        try
        {
            var streamingChatCompletionUpdates = await _apiTestService.InvokeNext(coPilotChatRequestMessage);

            HttpContext.Response.StatusCode = 200;
            await _responseStreamWriter.WriteToStreamAsync(HttpContext, streamingChatCompletionUpdates);
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
