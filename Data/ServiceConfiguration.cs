namespace Argus.Data
{
    public class ServiceConfiguration
    {
        public GitHubAuthenticationClientOptions GitHubAuthenticationClient { get; set; }
        public GitHubRawContentCdnClientOptions GitHubRawContentCdnClient { get; set; }
        public GitHubLLMQueryClientOptions GitHubLLMQueryClient { get; set; }
        public AzureEmbeddingClientOptions AzureEmbeddingClient { get; set; }
    }
}
