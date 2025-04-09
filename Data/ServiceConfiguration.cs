namespace Argus.Data
{
    public class ServiceConfiguration
    {
        public GitHubAuthenticationClientOptions GitHubAuthenticationClient { get; set; }
        public GitHubLLMQueryClientOptions GitHubLLMQueryClient { get; set; }
    }
}
