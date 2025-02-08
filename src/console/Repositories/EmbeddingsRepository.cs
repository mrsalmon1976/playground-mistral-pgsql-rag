using Mistral.Data;
using Mistral.Models;
using Dapper;
using System.ComponentModel;

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
