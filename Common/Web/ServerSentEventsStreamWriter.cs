using Argus.Contracts.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Argus.Common.Web;

public class ServerSentEventsStreamWriter : IResponseStreamWriter<ServerSentEventsStreamWriter>
{
    private readonly ILogger<ServerSentEventsStreamWriter> _logger;

    public ServerSentEventsStreamWriter(ILogger<ServerSentEventsStreamWriter> logger)
    {
        _logger = logger;
    }

    public void StartStream(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "text/event-stream";
    }


    public async Task WriteToStreamAsync(HttpContext httpContext, IReadOnlyList<object> messages)
    {
        foreach (var message in messages)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var messageString = "data: " + serializedMessage + "\n\n";
            await httpContext.Response.WriteAsync(messageString);
        }
        await httpContext.Response.Body.FlushAsync();
    }

    public async Task CompleteStream(HttpContext httpContext)
    {
        var completionMessage = "data: [DONE]\n\n";
        await httpContext.Response.WriteAsync(completionMessage);
        await httpContext.Response.Body.FlushAsync();
    }
}
