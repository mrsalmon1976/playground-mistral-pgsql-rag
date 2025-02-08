
using Dapper;
using Mistral;
using Mistral.Config;
using Mistral.Data;
using Mistral.Models;
using Mistral.Repositories;
using Pgvector.Dapper;

const string CrickLawChangeProposalDocumentId = "CRICKET_LAW_CHANGE_PROPOSAL";

SqlMapper.AddTypeHandler(new VectorTypeHandler());
AppSettings appSettings = AppSettings.Load();

using IDbContext dbContext = new DbContext(appSettings.DefaultConnectionString);
DocumentService documentService = new DocumentService(appSettings);
MistralService mistralService = new MistralService(appSettings);
EmbeddingsRepository embeddingsRepo = new EmbeddingsRepository(dbContext);
var modelMapper = new ModelMapper();

// split the documents into chunks
var splitDocuments = documentService.SplitDocument();
Console.WriteLine(splitDocuments.Count);
for (var i = 0; i < splitDocuments.Count; i++)
{
    Console.WriteLine($"Document {i}: {splitDocuments[i]}");
    Console.WriteLine("-------------------------------------------------------");
}

// get the embeddings for each chunk if we haven't stored them already
bool isDocumentPersisted = await embeddingsRepo.GetDocumentCount(CrickLawChangeProposalDocumentId) > 0;
if (!isDocumentPersisted)
{
    var embeddingDocuments = await mistralService.CreateEmbeddings(splitDocuments);

    // save the embeddings to the database
    var embeddings = embeddingDocuments.Select(x => modelMapper.ConvertEmbeddingDocumentToDbEmbedding(x));
    await embeddingsRepo.SaveEmbeddings(CrickLawChangeProposalDocumentId, embeddings);
}
// await mistralService.Chat();


Console.ReadLine();




