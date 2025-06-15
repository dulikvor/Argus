using Argus.Common.Data;
using System.Net;

namespace Argus.Common.Http
{
    public class HttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpResponseException(HttpStatusCode statusCode, string message) : base(message)
        {
            ArgumentValidationHelper.Ensure.NotNull(message, nameof(message));
            StatusCode = statusCode;
        }
    }
}
