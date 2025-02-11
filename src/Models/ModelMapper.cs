using Mistral.Utils;
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

        public EmbeddingDocument ConvertDbEmbeddingToEmbeddingDocument(DbEmbedding dbEmbedding)
        {
            List<decimal> embedding = dbEmbedding.Embedding.ToArray().Select(d => (decimal)d).ToList();

            return new EmbeddingDocument()
            {
                Index = int.Parse(dbEmbedding.DocumentId),
                Content = dbEmbedding.Content,
                Embedding = embedding
            };
        }
    }
}
