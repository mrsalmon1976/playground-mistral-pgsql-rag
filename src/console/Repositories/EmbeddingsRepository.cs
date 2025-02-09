using Mistral.Data;
using Mistral.Models;
using Dapper;
using Pgvector;

namespace Mistral.Repositories
{
    internal class EmbeddingsRepository
    {
        private readonly IDbContext _dbContext;


        public EmbeddingsRepository(IDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<int> GetDocumentCount(string documentId)
        {
            const string sql = $"SELECT COUNT(id) FROM embeddings WHERE document_id = @DocumentId";
            return await _dbContext.DbConnection.ExecuteScalarAsync<int>(sql, new { DocumentId = documentId });
        }

        public async Task<IEnumerable<EmbeddingComparison>> GetMatches(Vector vector, float threshold = 0.0F, int limit = 10)
        {
            string sql = @"SELECT 
                    id AS Id, 
                    content AS Content,
                    1 - (embedding <-> @Vector) AS Similarity
                FROM embeddings 
                WHERE 1 - (embedding <-> @Vector) > @Threshold
                ORDER BY embedding <-> @Vector LIMIT @Limit";
            return await _dbContext.DbConnection.QueryAsync<EmbeddingComparison>(sql, new { Vector = vector, Threshold = threshold, Limit = limit });
        }

        public async Task<IEnumerable<DbEmbedding>> GetNearestNeighbours(Vector vector, int limit = 5)
        {
            return await _dbContext.DbConnection.QueryAsync<DbEmbedding>("SELECT * FROM embeddings ORDER BY embedding <-> @Vector LIMIT @Limit", new { Vector = vector, Limit = limit });
        }

        public async Task SaveEmbeddings(string documentId, IEnumerable<DbEmbedding> embeddings)
        {
            string sql = @$"INSERT INTO embeddings 
                (document_id, content, embedding) 
                VALUES 
                ('{documentId}', @Content, @Embedding)";
            await _dbContext.DbConnection.ExecuteAsync(sql, embeddings);

        }
    }
}
