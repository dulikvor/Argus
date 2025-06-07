using ApiTestingAgent.Services;
using Argus.Clients.LLMQuery;
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
    private readonly IAzureLLMQueryClient _llmQueryClient;
    private readonly IResponseStreamWriter<ServerSentEventsStreamWriter> _responseStreamWriter;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        IApiTestService apiTestService,
        IAzureLLMQueryClient llmQueryClient,
        IResponseStreamWriter<ServerSentEventsStreamWriter> responseStreamWriter,
        ILogger<AgentController> logger)
    {
        _apiTestService = apiTestService;
        _llmQueryClient = llmQueryClient;
        _responseStreamWriter = responseStreamWriter;
        _logger = logger;
    }

    [HttpPost("/nextEvent")]
    public async Task NextEvent([FromBody] CoPilotChatRequestMessage coPilotChatRequestMessage)
    {
        try
        {
            _responseStreamWriter.StartStream(HttpContext);
            HttpContext.Response.StatusCode = 200;
            await _apiTestService.InvokeNext(HttpContext, coPilotChatRequestMessage);

            await _responseStreamWriter.CompleteStream(HttpContext);
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
