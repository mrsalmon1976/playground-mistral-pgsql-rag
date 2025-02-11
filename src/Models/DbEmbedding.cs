using Pgvector;


namespace Mistral.Models
{
    internal class DbEmbedding
    {
        public long Id { get; set; }

        public string DocumentId { get; set; } = String.Empty;

        public string Content { get; set; } = String.Empty;

        public Vector Embedding { get; set; } = new Vector(new float[] { 1, 1, 1 });
    }
}
