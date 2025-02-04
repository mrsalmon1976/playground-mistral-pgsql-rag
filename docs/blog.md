This post covers how to use Mistral's large language models (LLMs) via API to ask questions using local data stored in a PostgreSQL database with .NET.

Technologies used:
- [Mistral.AI](https://mistral.ai/technology/#models) - offers local and online high quality LLMs 
- [Ubuntu-22.04](https://ubuntu.com/download) running on WSL2
- [dotnet v8.0.12](https://dotnet.microsoft.com/en-us/download) - installed with command `sudo snap install dotnet-sdk --classic`

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
    new ChatMessage(ChatMessage.RoleEnum.System,
        "You are a professional cricket umpire who values brevity."),
    new ChatMessage(ChatMessage.RoleEnum.User,
        "What are three ways a batsman can be dismissed?")
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
