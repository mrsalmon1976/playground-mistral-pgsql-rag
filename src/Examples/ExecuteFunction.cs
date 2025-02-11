using Mistral.Config;
using Mistral.SDK;
using Mistral.SDK.Common;
using Mistral.SDK.DTOs;
using Mistral.Utils;

namespace Mistral.Examples
{

    public enum Gender
    {
        Male,
        Female
    }

    internal class ExecuteFunction
    {

        [Function("This function returns whether a person will have insurance coverage, based on their age and gender")]
        public static async Task<string> GetEligibility([FunctionParameter("Age of applicant", true)] int age,
        [FunctionParameter("Gender", true)] Gender gender)
        {
            await Task.Yield();

            if (age < 18)
            {
                return "Applicant must be 18 years or older";
            }
            if (gender == Gender.Male && age > 60)
            {
                return "Male applicants must be between 18 and 60 years of age";
            }
            if (gender == Gender.Female && age > 70)
            {
                return "Female applicants must be between 18 and 70 years of age";
            }

            return "This applicant is eligible for cover because they meet the age and gender criteria.";
        }

        public static async Task Run(AppSettings appSettings)
        {
            var client = new MistralClient(appSettings.MistralApiKey);

            while (true)
            {

                Console.WriteLine("Please describe the applicant.  Their age and gender are important - you can keep it brief.");
                Console.WriteLine("\tExamples:");
                Console.WriteLine("\t\tMy son is 15 years old");
                Console.WriteLine("\t\tMy mother is 65 years old");
                Console.WriteLine("\t\tMy infant daughter is wanting insurance");
                Console.WriteLine("\t\tMy colleague is retiring next year as my company has mandatory retirement at 63.  She is wanting insurance.");

                Console.Write(">>");
                var applicant = Console.ReadLine();

                var messages = new List<ChatMessage>()
                {
                    new ChatMessage(ChatMessage.RoleEnum.User, $"The applicant is described as \"{applicant}\" - are they eligible for insurance cover?  Please provide the reason for the decision.")
                };
                var request = new ChatCompletionRequest(ModelDefinitions.MistralSmall, messages);
                request.MaxTokens = 1024;
                request.Temperature = 0.0m;
                request.ToolChoice = ToolChoiceType.Auto;

                request.Tools = Mistral.SDK.Common.Tool.GetAllAvailableTools(includeDefaults: false, forceUpdate: true, clearCache: true).ToList();

                var response = await client.Completions.GetCompletionAsync(request).ConfigureAwait(false);

                messages.Add(response.Choices.First().Message);

                // note how we have not hard-coded a tool to be called - the response contains data telling us that we should
                // execute the tool
                foreach (var toolCall in response.ToolCalls)
                {
                    var resp = await toolCall.InvokeAsync<string>();
                    messages.Add(new ChatMessage(toolCall, resp));
                }

                if (response.ToolCalls.Count == 0)
                {
                    ConsoleUtils.WriteLine("No tool calls were made - perhaps try more detail.", ConsoleColor.Red);
                }
                else
                {

                    var response2 = await client.Completions.GetCompletionAsync(request).ConfigureAwait(false);
                    ConsoleUtils.WriteLine(response2.Choices.First().Message.Content, ConsoleColor.Green);
                }
                Console.WriteLine("");
            }

        }
    }
}
