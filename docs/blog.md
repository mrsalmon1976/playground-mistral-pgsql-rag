This post covers how to use Mistral's large language models (LLMs) via API to ask questions using local data stored in a PostgreSQL database with .NET.

Technologies used:
- [Mistral.AI](https://mistral.ai/technology/#models) - offers local and online high quality LLMs 
- [dotnet v8.0.12](https://dotnet.microsoft.com/en-us/download) - installed with command `sudo snap install dotnet-sdk --classic`
- [PostgreSQL](http://postgresql.org/) - with vector extension 
  - I used the [ankane/pgvector](https://hub.docker.com/r/ankane/pgvector) image for this proof of concept
  - Consider [Supabase](https://supabase.com/) if you don't want to run your own database
- [Ubuntu-22.04](https://ubuntu.com/download) running on WSL2
- [Dapper](https://github.com/DapperLib/Dapper)
- [PgVector](https://github.com/pgvector/pgvector-dotnet) and Pgvector.Dapper - provides .NET support for working with vectors with PostgreSQL

# Connecting to the Mistral API

To start working, you will need to:

1. Sign up with [Mistral](https://mistral.ai/) and create a developer account
2. Create a .NET 8 console application, and add the `Mistral.SDK` nuget package to your solution
3. Create an API key on the Mistral site, and add it to your app settings.  A key will look something like `xh53nppoL8uRmT...................`

For now, set up a simple .NET 8 console application, that uses the API key to send a simple question through to Mistral

```csharp
using Mistral.Config;
using Mistral.SDK;
using Mistral.SDK.DTOs;

AppSettings appSettings = AppSettings.Load();

var client = new MistralClient(appSettings.MistralApiKey);
var request = new ChatCompletionRequest(
    //define model - required
    ModelDefinitions.MistralMedium,
    //define messages - required
    new List<ChatMessage>()
    {
        // optional - but useful for setting the tone of the conversation
        new ChatMessage(ChatMessage.RoleEnum.System
            , "You are a professional cricket umpire who values brevity."),
        new ChatMessage(ChatMessage.RoleEnum.User
            , "What are three ways a batsman can be dismissed?")
    },
    // optional (0 - 1) - defaults to 0.7 where lower is more focused, higher more random
    temperature: 0.7M,
    );

var response = await client.Completions.GetCompletionAsync(request);
Console.WriteLine(response.Choices.First().Message.Content);
```

# Streaming Response Content

If you want to stream the response for a better user experience, this is done as follows:

```csharp
var streamedResponse = client.Completions.StreamCompletionAsync(request);
await foreach (var chunk in streamedResponse)
{
    Console.Write(chunk.Choices[0].Delta.Content);
}
```

# LLM Overview

TODO: Give a brief overview of how an LLM works

# Retrieval-Augmented Generation (RAG)

Given the above, if we want to load our document as context, we are going to need to 
implement a RAG process so context can be built when the user asks a question.

To do this, we need to store our document in vector format i.e. create embeddings.  However, 
there is a problem here - embeddings that are too large become far too broad and are not useful.
A better approach is to break your document up into chunks, and create an embedding for each 
of these chunks.  To do this, we use [LangChain](https://www.langchain.com/).

## Splitting Text with LangChain

LangChain itself is a Python library, but fortunately there is a [C# port](https://github.com/tryAGI/LangChain) with 
an [associated NuGet package](https://www.nuget.org/packages/LangChain).  This can be used to split 
large documents into chunks that can be used for your RAG process.  While there are no hard and fast 
rules and the best results will require some trial and error, these basic rules apply:

- Shorter chunks capture precise meanings, but may miss wider context
- Longer chunks mean there is more context to answer questions, but can produce too broad a scope of information and may reduce the quality of the similarity search that the vector database does (the underlying value of the embedding may be ambiguous)
- You should optimise for the smallest possible size that doesn't lose context

In the example, there are some rather preposterous proposals for changes to the laws of cricket, which in a text document of 963 characters, with each proposal on a new line.  I wasn't sure of the optimal approach for chunking for this document, as your chunk size and overlap are important for optimising understanding.

Initially I set it with a chunk size of 250 and a chunk overlap of 40, but looking at the documents that this created made me question this, and I also considered scrapping chunk sizing/overlapping and using just plan separators for the splitting.  

Eventually, I decided to use AI to tell me how to use AI, so I asked ChatGPT:

> I have a text document of 963 characters, made up of 9 lines proposing new laws.  I want to use this for a RAG example - what do you suggest the chunk size and overlap should be?  I can paste the document if that helps.

Without the document contents, it suggested using EOL separators, but after pasting my document it recommended a chunk size of 250-300 characters, and an overlap of 50-100 characters, with some detailed reasoning provided of how it came to this conclusion.  This resulted in the following code:

```csharp
string[] text = { File.ReadAllText(_appSettings.CricketLawChangeProposalDocumentPath) };
var texts = new ReadOnlyCollection<string>(text);

var splitter = new LangChain.Splitters.Text.RecursiveCharacterTextSplitter(chunkSize: 275, chunkOverlap: 50);

var splitDocuments = splitter.CreateDocuments(texts);

Console.WriteLine(splitDocuments.Count);
for (var i=0; i< splitDocuments.Count; i++)
{
    Console.WriteLine($"Document {i}: {splitDocuments[i].PageContent}");
    Console.WriteLine("-------------------------------------------------------");
}
```

This results in 6 chunks, with overlap between the chunks.

## Converting Text Chunks into Embeddings

This is done easily using the Mistral Embeddings model via the API.  In my code example below, I use the API to create the embeddings, but also associate the embeddings with the original 
text chunk that the embedding relates to:

```csharp
internal async Task<IEnumerable<EmbeddingDocument>> CreateEmbeddings(List<string> input)
{
    var client = new MistralClient(_appSettings.MistralApiKey);
    var request = new EmbeddingRequest(ModelDefinitions.MistralEmbed, input);
     
    var response = await client.Embeddings.GetEmbeddingsAsync(request);

    var result = new List<EmbeddingDocument>();
    for (var i = 0; i < input.Count; i++)
    {
        result.Add(new EmbeddingDocument()
        {
            Index = i,
            Content = input[i],
            Embedding = response.Data[i].Embedding
        });
    }
    return result;
}
```

The `EmbeddingDocument` is a simple POCO as follows:

```csharp
internal class EmbeddingDocument
{
    public int Index { get; set; }

    public string Content { get; set; } = String.Empty;

    public List<decimal> Embedding { get; set; } = new List<decimal>();
}
```

## Storing the Embeddings in PosgreSQL

We want to store our vectors in a database, so we can query for similarities and add relevant embeddings to our context. 

First, create a table to store the embeddings.

```sql
CREATE TABLE embeddings (
	id bigserial primary key,
	document_id text,           -- used to group embeddings
	content text,               -- text content of the embedding
	embedding vector(1024)      -- the dimension of our embedding
);
```

Note that the `document_id` should really be a foreign key, but for purposes of the example is just a text identifier that we will hard-code.

I am using Dapper for my queries, so I need to

1. Add PgVector to my solution
2. Add PgVector.Dapper to my solution
3. Ensure the Vector type is loaded for Dapper:
    ```csharp
    SqlMapper.AddTypeHandler(new VectorTypeHandler());
    ```
4. Create a model that maps to my database table
   ```csharp
   internal class DbEmbedding
    {
        public string DocumentId { get; set; } = String.Empty;

        public string Content { get; set; } = String.Empty;

        public Vector Embedding { get; set; } = new Vector(new float[] { 1, 1, 1 });
    }
    ```
5. Create a mapper to map models with different properties
   ```csharp
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
    ```
6. Write an insert method for persisting the embeddings:
   ```csharp
    public async Task SaveEmbeddings(string documentId, IEnumerable<DbEmbedding> embeddings)
    {
        string sql = @$"INSERT INTO embeddings 
            (document_id, content, embedding) 
            VALUES 
            ('{documentId}', @Content, @Embedding)";
        await _dbContext.DbConnection.ExecuteAsync(sql, embeddings);
    }
    ```
7. Execute!
   ```csharp
       var embeddings = embeddingDocuments.Select(x => modelMapper.ConvertEmbeddingDocumentToDbEmbedding(x));
    await embeddingsRepo.SaveEmbeddings("CRICKET_LAW_CHANGE_PROPOSAL", embeddings);
    ```

If you look in your `embeddings` table, you should now see multiple records for your document - my law change proposal document generated 6 rows in the database.





