using Microsoft.AspNetCore.Http;
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

    public async Task WriteToStreamAsync(HttpContext httpContext, IReadOnlyList<object> messages)
    {
        httpContext.Response.ContentType = "text/event-stream";

        foreach (var message in messages)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var messageString = "data: " + serializedMessage + "\n\n";

            await httpContext.Response.WriteAsync(messageString);
        }

        var completionMessage = "data: [DONE]\n\n";
        await httpContext.Response.WriteAsync(completionMessage);
        await httpContext.Response.Body.FlushAsync();
    }
}
