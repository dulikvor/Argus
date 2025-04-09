using Argus.Common.Data;
using System.Net.Http.Headers;

namespace Argus.Common.Clients
{
    public class HttpClientAuthenticationHandler : DelegatingHandler
    {
        Aliases.TokenCreator _tokenCreator;

        public HttpClientAuthenticationHandler(Aliases.TokenCreator tokenCreator)
        {
            _tokenCreator = tokenCreator;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolvedToken = await _tokenCreator();
            request.Headers.Authorization = new AuthenticationHeaderValue(resolvedToken.Schema, resolvedToken.Token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
