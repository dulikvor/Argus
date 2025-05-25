using Argus.Common.PromptDescriptors;
using Argus.Contracts.Semantic;
using System.Text;

namespace Argus.Common.Builtin.PromptDescriptor
{
    public class ContextPromptDescriptor : BasePromptDescriptor
    {
        public override string DescriptorType => nameof(ContextPromptDescriptor);

        public ContextPromptDescriptor()
        {
            Initialize();
        }

        protected override void Initialize()
        {
            // Initialize prompts
            Prompts.Add(PromptsConstants.Prompts.Keys.Context,
                "You are a knowledgeable and helpful assistant. Use the provided context to answer user questions accurately and clearly. If the context does not contain enough information, say so rather than guessing."
               );
        }

        public string ReconcilePrompt(IEnumerable<SemanticEntity> semanticEntities)
        {
            var sb = new StringBuilder();

            var prompt = GetPrompt(PromptsConstants.Prompts.Keys.Context);
            sb.AppendLine(prompt);
            sb.AppendLine("Context:");
            foreach (var semanticEntity in semanticEntities)
            {
                sb.AppendLine($"Input: {semanticEntity.InputText}");
                sb.AppendLine($"Output: {semanticEntity.OutputText}");
            }
            return sb.ToString();
        }
    }
}
