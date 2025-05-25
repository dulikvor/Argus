using Argus.Contracts.OpenAI;

namespace Argus.Clients.AzureEmbeddingClient
{
    public interface IAzureEmbeddingClient
    {
        Task<EmbeddingResponse> GenerateEmbeddingAsync(EmbeddingRequest embeddingRequest);
    }
}