using Argus.Common.Data;
using Argus.Common.Http;
using Argus.Common.Retrieval;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;

namespace Argus.Clients.AzureEmbeddingClient
{
    public class AzureEmbeddingClient : IAzureEmbeddingClient
    {
        private readonly HttpClient _httpClient;

        public AzureEmbeddingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest embeddingRequest)
        {
            using var activityScope = ActivityScope.Create(nameof(SemanticStore));
            return await activityScope.Monitor(async () =>
            {
                ArgumentValidationHelper.Ensure.NotNull(embeddingRequest, nameof(embeddingRequest));

                var headers = new Dictionary<string, string>
                {
                    { "User-Agent", "Argus" }
                };

                return await _httpClient.PostAsync<EmbeddingResponse, EmbeddingRequest>("embeddings?api-version=2023-05-15", embeddingRequest, headers);
            });
        }
    }
}