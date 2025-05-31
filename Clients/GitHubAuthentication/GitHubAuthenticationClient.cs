using Argus.Common.Clients;
using Argus.Common.Data;
using Argus.Contracts;
using Argus.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Argus.Clients.GitHubAuthentication
{
    public class GitHubAuthenticationClient : IGitHubAuthenticationClient
    {
        private readonly HttpClient _httpClient;

        public GitHubAuthenticationClient(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public static Task<(string Schema, string Token)> TokenCreator(string scope)
        {
            var token = CallContext.GetData(ServiceConstants.Authentication.GitHubTokenKey) as string;
            return Task.FromResult((JwtBearerDefaults.AuthenticationScheme, token!));
        }

        public async Task<GitHubAuthenticationContract> GetUserAsync()
        {
            var headers = new Dictionary<string, string>
            {
                { "User-Agent", "Argus" }
            };
            return await _httpClient.GetAsync<GitHubAuthenticationContract>("user", headers);
        }
    }
}
