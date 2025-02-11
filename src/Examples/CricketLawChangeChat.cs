using Mistral.Config;
using Mistral.SDK.DTOs;
using Mistral.SDK;

namespace Mistral.Examples
{
    internal class CricketLawChangeChat
    {
        public static async Task Run(AppSettings appSettings)
        {
            using var client = new MistralClient(appSettings.MistralApiKey);

            Console.WriteLine();
            Console.WriteLine("Ask as many questions as you like about the new proposals.");

            string context = File.ReadAllText(appSettings.CricketLawChangeProposalDocumentPath);
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
                    await Chat(client, context, question);
                }
                Console.WriteLine();
            }
        }

        private static async Task Chat(MistralClient client, string context, string question)
        {
            var request = new ChatCompletionRequest(
                //define model - required
                ModelDefinitions.MistralSmall,
                //define messages - required
                new List<ChatMessage>()
                {
                new ChatMessage(ChatMessage.RoleEnum.System, "You are a professional cricket umpire."),
                new ChatMessage(ChatMessage.RoleEnum.User, $"New law proposals: {context} - Question: {question}")
                },
                //optional - defaults to 0.7, determines the creativity of the response (lower values are more focused, higher values up to 1 are more random)
                temperature: 0.7M
            );

            //var response = await client.Completions.GetCompletionAsync(request);
            //Console.WriteLine(response.Choices.First().Message.Content);

            var streamedResponse = client.Completions.StreamCompletionAsync(request);
            await foreach (var chunk in streamedResponse)
            {
                Console.Write(chunk.Choices[0].Delta.Content);
            }
        }


    }
}
