namespace Argus.Common.Web;

public interface IResponseStreamWriter<T> where T : IResponseStreamWriter<T>
{
    Task WriteToStreamAsync(HttpContext httpContext, IReadOnlyList<object> messages);
}