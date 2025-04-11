using Microsoft.Extensions.DependencyInjection;

namespace Argus.Common.Clients
{
    public static class HttpClientBuilderExtension
    {
        public static IHttpClientBuilder HttpClientConfiguration(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder
                .SetHandlerLifetime(TimeSpan.FromHours(1));
        }
    }
}
