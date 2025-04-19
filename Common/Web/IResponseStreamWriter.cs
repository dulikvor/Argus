using Microsoft.AspNetCore.Http;

namespace Argus.Common.Web;

public interface IResponseStreamWriter<T> where T : IResponseStreamWriter<T>
{
    void StartStream(HttpContext httpContext);
    Task WriteToStreamAsync(HttpContext httpContext, IReadOnlyList<object> messages);
    Task CompleteStream(HttpContext httpContext);
}