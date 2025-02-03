
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


