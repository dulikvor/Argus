using Argus.Contracts;

namespace Argus.Clients.GitHubRawContentCdnClient
{
    public interface IGitHubRawContentCdnClient
    {
        public Task<string> GetRawContent(string user, string repo, string branch, string pathToFile);
    }
}
