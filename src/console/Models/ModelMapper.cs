using Pgvector;

namespace Mistral.Models
{
    internal class ModelMapper
    {
        public DbEmbedding ConvertEmbeddingDocumentToDbEmbedding(EmbeddingDocument embeddingDocument)
        {
            float[] embedding = embeddingDocument.Embedding.Select(d => (float)d).ToArray();

            return new DbEmbedding()
            {
                DocumentId = embeddingDocument.Index.ToString(),
                Content = embeddingDocument.Content,
                Embedding = new Vector(embedding)
            };
        }
    }
}
