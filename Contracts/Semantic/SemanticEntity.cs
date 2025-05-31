namespace Argus.Contracts.Semantic
{
    public class SemanticEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InputText { get; set; }
        public string OutputText { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public Dictionary<string, string> Metadata { get; set; }
    }
}
