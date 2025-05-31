using Argus.Common.Data;
using System.Net.Http.Headers;

namespace Argus.Common.Clients
{
    public class HttpClientAuthenticationHandler : DelegatingHandler
    {
        Aliases.TokenCreator _tokenCreator;
        private readonly string _scope;

        public HttpClientAuthenticationHandler(string scope, Aliases.TokenCreator tokenCreator)
        {
            _tokenCreator = tokenCreator;
            _scope = scope;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolvedToken = await _tokenCreator(_scope);
            request.Headers.Authorization = new AuthenticationHeaderValue(resolvedToken.Schema, resolvedToken.Token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
