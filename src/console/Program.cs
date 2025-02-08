
using Mistral;
using Mistral.Config;

AppSettings appSettings = AppSettings.Load();

DocumentService documentService = new DocumentService(appSettings);
MistralService mistralService = new MistralService(appSettings);

// split the documents into chunks
var splitDocuments = documentService.SplitDocument();
Console.WriteLine(splitDocuments.Count);
for (var i = 0; i < splitDocuments.Count; i++)
{
    Console.WriteLine($"Document {i}: {splitDocuments[i]}");
    Console.WriteLine("-------------------------------------------------------");
}

// get the embeddings for each chunk
var embeddings = await mistralService.CreateEmbeddings(splitDocuments);

// await mistralService.Chat();


Console.ReadLine();




