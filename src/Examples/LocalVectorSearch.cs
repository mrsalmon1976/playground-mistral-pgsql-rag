using Dapper;
using LangChain.Extensions;
using Mistral.Config;
using Mistral.Data;
using Mistral.Models;
using Mistral.Repositories;
using Mistral.SDK;
using Mistral.SDK.DTOs;
using Mistral.Utils;
using Pgvector;
using Pgvector.Dapper;
using System.Collections.ObjectModel;
using System.Data;

namespace Mistral.Examples
{
    internal class LocalVectorSearch
    {
        public static async Task Run(AppSettings appSettings)
        {
            const string CricketBookDocumentId = "CRICKET";

            SqlMapper.AddTypeHandler(new VectorTypeHandler());

            using IDbContext dbContext = new DbContext(appSettings.DefaultConnectionString);
            using var client = new MistralClient(appSettings.MistralApiKey);

            EmbeddingsRepository embeddingsRepo = new EmbeddingsRepository(dbContext);
            var modelMapper = new ModelMapper();


            // get the embeddings for each chunk if we haven't stored them already
            bool isDocumentPersisted = await embeddingsRepo.GetDocumentCount(CricketBookDocumentId) > 0;
            if (!isDocumentPersisted)
            {

                // split the documents into chunks
                var splitDocuments = SplitDocument(appSettings.CricketBookPath, 400, 50);
                Console.WriteLine($"Document split into {splitDocuments.Count} chunks");

                Console.Write("Creating embeddings for each chunk...");
                var embeddingDocuments = await CreateEmbeddings(client, splitDocuments);
                Console.WriteLine("done.");

                // save the embeddings to the database
                Console.Write("Saving embeddings to the database...");
                var embeddings = embeddingDocuments.Select(x => modelMapper.ConvertEmbeddingDocumentToDbEmbedding(x));
                await embeddingsRepo.SaveEmbeddings(CricketBookDocumentId, embeddings);
                Console.WriteLine("done.");
            }
            else
            {
                Console.WriteLine("Embeddings already created - run \"delete from embeddings\" on the database to clear.");
            }

            // asking questions about the book 

            Console.WriteLine();
            Console.WriteLine("What would you like to search for in the book 'Cricket'?  This is a vector search, so will look for similar phrases and words");

            string question = String.Empty;

            while (true)
            {
                Console.Write(">> ");
                question = (Console.ReadLine() ?? String.Empty);
                if (question.ToLower() == "exit")
                {
                    Console.WriteLine("Thanks for playing!");
                    break;
                }

                if (!String.IsNullOrEmpty(question))
                {
                    var questionEmbedding = (await CreateEmbeddings(client, new List<string> { question })).First();
                    float[] embedding = questionEmbedding.Embedding.Select(d => (float)d).ToArray();
                    Vector questionVector = new Vector(embedding);

                    var matches = await embeddingsRepo.GetMatches(questionVector, 0.2F, 5);
                    Console.WriteLine($"Matches: {matches.Count()}");

                    int matchNum = 1;
                    foreach (var match in matches)
                    {
                        ConsoleUtils.WriteLine($"Match {matchNum}", ConsoleColor.Green);
                        Console.WriteLine(match.Content);
                        matchNum++;
                    }


                }
                Console.WriteLine();
            }

        }

        private static async Task<IEnumerable<EmbeddingDocument>> CreateEmbeddings(MistralClient client, List<string> input, int maxDocumentsPerBatch = 100)
        {
            var result = new List<EmbeddingDocument>();

            // Process input in batches
            for (var i = 0; i < input.Count; i += maxDocumentsPerBatch)
            {
                // Get the current batch of input
                var batch = input.Skip(i).Take(maxDocumentsPerBatch).ToList();

                // Create the embedding request for the current batch
                var request = new EmbeddingRequest(ModelDefinitions.MistralEmbed, batch);

                // Get embeddings for the current batch
                var response = await client.Embeddings.GetEmbeddingsAsync(request);

                // Add the results to the list
                for (var j = 0; j < batch.Count; j++)
                {
                    result.Add(new EmbeddingDocument()
                    {
                        Index = i + j,
                        Content = batch[j],
                        Embedding = response.Data[j].Embedding
                    });
                }
            }

            return result;
        }


        private static List<string> SplitDocument(string documentPath, int chunkSize, int chunkOverlap)
        {
            string[] text = { File.ReadAllText(documentPath) };
            var texts = new ReadOnlyCollection<string>(text);

            var splitter = new LangChain.Splitters.Text.RecursiveCharacterTextSplitter(chunkSize: chunkSize, chunkOverlap: chunkOverlap);

            var splitDocuments = splitter.CreateDocuments(texts);

            return splitDocuments.Select(x => x.PageContent).ToList();


        }

    }
}
