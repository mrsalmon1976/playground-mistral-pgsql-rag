
using Mistral.Config;
using Mistral.SDK;
using Mistral.SDK.DTOs;

AppSettings appSettings = AppSettings.Load();

var client = new MistralClient(appSettings.MistralApiKey);
var request = new ChatCompletionRequest(
    //define model - required
    ModelDefinitions.MistralSmall,
    //define messages - required
    new List<ChatMessage>()
{
    new ChatMessage(ChatMessage.RoleEnum.System,
        "You are a grumpy professional cricket umpire."),
    new ChatMessage(ChatMessage.RoleEnum.User,
        "What are three ways a batsman can be dismissed?")
},
    //optional - defaults to false
    safePrompt: true,
    //optional - defaults to 0.7, determines the creativity of the response (lower values are more focused, higher values up to 1 are more random)
    temperature: 0.7M,
    //optional - defaults to null
    //maxTokens: 500,
    //optional - defaults to 1
    topP: 1,
    //optional - defaults to null
    randomSeed: 32);

var response = await client.Completions.GetCompletionAsync(request);
Console.WriteLine(response.Choices.First().Message.Content);

var streamedResponse = client.Completions.StreamCompletionAsync(request);
await foreach (var chunk in streamedResponse)
{
    Console.Write(chunk.Choices[0].Delta.Content);
}




