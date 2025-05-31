using Argus.Contracts.Semantic;

namespace Argus.Common.Retrieval
{
    public interface ISemanticStore
    {
        void Add(string inputText, string outputText, Dictionary<string, string> metadata = default);
        Task<List<SemanticEntity>> Search(string inputText, int topK = 5);
    }
}
