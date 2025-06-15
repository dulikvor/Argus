using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Argus.Common.Web;

public class ServerSentEventsStreamWriter : IResponseStreamWriter<ServerSentEventsStreamWriter>
{
    public ServerSentEventsStreamWriter()
    {
    }

    public void StartStream(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "text/event-stream";
    }


    public async Task WriteToStreamAsync(HttpContext httpContext, IReadOnlyList<object> messages, EventType eventType = EventType.Message)
    {
        foreach (var message in messages)
        {
            var serializedMessage = JsonSerializer.Serialize(message);
            var messageString = eventType == EventType.Message
                ? "data: " + serializedMessage + "\n\n"
                : "event: " + eventType.ToSerializedString() + "\ndata: " + serializedMessage + "\n\n";
            await httpContext.Response.WriteAsync(messageString);
            await httpContext.Response.Body.FlushAsync();
        }
    }

    public async Task CompleteStream(HttpContext httpContext)
    {
        var completionMessage = "data: [DONE]\n\n";
        await httpContext.Response.WriteAsync(completionMessage);
        await httpContext.Response.Body.FlushAsync();
    }
}