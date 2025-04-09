using Argus.Contracts;

namespace Argus.Clients.GitHubAuthentication
{
    public interface IGitHubAuthenticationClient
    {
        public Task<GitHubAuthenticationContract> GetUserAsync();
    }
}
