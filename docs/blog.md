This post covers how to use Mistral's large language models (LLMs) via API to ask questions using local data stored in a PostgreSQL database with .NET.

Technologies used:
- [Mistral.AI](https://mistral.ai/technology/#models) - offers local and online high quality LLMs 
- [Ubuntu-22.04](https://ubuntu.com/download) running on WSL2
- [dotnet v8.0.12](https://dotnet.microsoft.com/en-us/download) - installed with command `sudo snap install dotnet-sdk --classic`

# Connecting to the Mistral API

- Sign up with [Mistral](https://mistral.ai/) and create a developer account
- Create a .NET 8 console application, and add the `Mistral.SDK` nuget package to your solution
- Create an API on the Mistral site, and add it to your app settings.  A key will look something like `  "MistralApiKey": "xh53nppoL8uRmT..................."
- For now, set up a simple .NET 8 console application, that uses the API key to send a simple question through to Mistral
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
      new ChatMessage(ChatMessage.RoleEnum.System,
          "You are a professional cricket umpire who values brevity."),
      new ChatMessage(ChatMessage.RoleEnum.User,
          "What are three ways a batsman can be dismissed?")
  },
      //optional - defaults to false
      safePrompt: true,
      //optional - defaults to 0.7
      temperature: 0.7M,
      //optional - defaults to null
      maxTokens: 500,
      //optional - defaults to 1
      topP: 1,
      //optional - defaults to null
      randomSeed: 32);
  var response = await client.Completions.GetCompletionAsync(request);
  Console.WriteLine(response.Choices.First().Message.Content);
  ```

  In this example, the `ChatMessage.RoleEnum.System` declaration is not required, but is useful for setting the tone of your bot.
