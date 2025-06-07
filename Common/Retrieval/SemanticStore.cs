using Argus.Clients.AzureEmbeddingClient;
using Argus.Common.Data;
using Argus.Common.Telemetry;
using Argus.Contracts.OpenAI;
using Argus.Contracts.Semantic;
using Argus.Data;
using System.Collections.Concurrent;

namespace Argus.Common.Retrieval
{
    public class SemanticStore : ISemanticStore
    {
        private readonly IAzureEmbeddingClient _azureEmbeddingClient;
        private readonly ConcurrentDictionary<string, List<SemanticEntity>> _store = new();

        public SemanticStore(IAzureEmbeddingClient azureEmbeddingClient)
        {
            _azureEmbeddingClient = azureEmbeddingClient;
        }

        public void Add(string inputText, string outputText, Dictionary<string, string> metadata = default)
        {
            var userName = CallContext.GetData(ServiceConstants.Authentication.UserNameKey) as string;

            if (string.IsNullOrEmpty(inputText))
                throw new ArgumentException("input text cannot be null or empty.", nameof(inputText));

            if (string.IsNullOrEmpty(outputText))
                throw new ArgumentException("output text cannot be null or empty.", nameof(outputText));

            if (string.IsNullOrEmpty(userName))
                throw new InvalidOperationException("User name is not available in the call context.");

            Task.Run(async () =>
            {
                var entity = await CreateEntity(inputText, outputText, metadata);
                _store.AddOrUpdate(userName,
                    _ => new List<SemanticEntity> { entity },
                    (_, existingList) =>
                    {
                        existingList.Add(entity);
                        return existingList;
                    });
            });
        }

        public async Task<List<SemanticEntity>> Search(string inputText, int topK = 5)
        {
            using var activityScope = ActivityScope.Create(nameof(SemanticStore));
            return await activityScope.Monitor(async () =>
            {
                var userName = CallContext.GetData(ServiceConstants.Authentication.UserNameKey) as string;

                if (string.IsNullOrEmpty(inputText))
                    throw new ArgumentException("Input text cannot be null or empty.", nameof(inputText));

                if (string.IsNullOrEmpty(userName))
                    throw new InvalidOperationException("User name is not available in the call context.");

                var embeddingResult = await _azureEmbeddingClient.GenerateEmbeddingAsync(new EmbeddingRequest()
                {
                    Input = inputText
                });

                var semanticsEntities = _store.ContainsKey(userName) ? _store[userName] : new List<SemanticEntity>();

                var results = new List<(SemanticEntity Entity, double Similarity)>();
                using var activityScope1 = ActivityScope.Create(nameof(SemanticStore), "Scope1");
                {
                    var semaphore = new SemaphoreSlim(100);
                    var tasks = semanticsEntities.Select(entity =>
                    {
                        return Task.Run(async () =>
                        {
                            await semaphore.WaitAsync().ConfigureAwait(false);
                            try
                            {
                                var similarity = VectorUtils.CosineSimilarity(entity.Embedding, embeddingResult.Data.First().EmbeddingVector.ToArray());
                                lock (results)
                                {
                                    results.Add((entity, similarity));
                                }
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                    }).ToList();

                    await Task.WhenAll(tasks);
                }


            using var activityScope2 = ActivityScope.Create(nameof(SemanticStore), "Scope2");
            {
                return results
                    .OrderByDescending(x => x.Similarity)
                    .Take(topK)
                    .Select(x => x.Entity)
                    .ToList();
            }
            });
        }

        private async Task<SemanticEntity> CreateEntity(string inputText, string outputText, Dictionary<string, string> metadata = default)
        {
            var embedding = await _azureEmbeddingClient.GenerateEmbeddingAsync(new EmbeddingRequest()
            {
                Input = inputText
            });

            return new SemanticEntity()
            {
                InputText = inputText,
                OutputText = outputText,
                Embedding = embedding.Data.First().EmbeddingVector,
                Metadata = metadata
            };
        }
    }
}