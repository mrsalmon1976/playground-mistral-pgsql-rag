using Mistral.SDK.DTOs;
using Mistral.SDK;
using System;
using Mistral.Config;
using Mistral.Models;

namespace Mistral
{
    internal class MistralService
    {
        private readonly AppSettings _appSettings;

        public MistralService(AppSettings appSettings) 
        {
            this._appSettings = appSettings;
        }
        public async Task Chat()
        {
            var client = new MistralClient(_appSettings.MistralApiKey);
            var request = new ChatCompletionRequest(
                //define model - required
                ModelDefinitions.MistralSmall,
                //define messages - required
                new List<ChatMessage>()
                {
                new ChatMessage(ChatMessage.RoleEnum.System, "You are a grumpy professional cricket umpire."),
                new ChatMessage(ChatMessage.RoleEnum.User, "What are three ways a batsman can be dismissed?")
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

    }


}
