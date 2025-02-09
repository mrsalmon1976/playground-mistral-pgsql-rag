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

                Console.WriteLine("How old is the applicant? (Valid values 1 - 120)");
                Console.Write(">> ");
                string ageInput = Console.ReadLine() ?? String.Empty;
                if (ageInput.ToLower() == "exit")
                {
                    return;
                }

                int.TryParse(ageInput, out int age);
                if (age <= 0 || age > 120)
                {
                    ConsoleUtils.WriteLine("Invalid age", ConsoleColor.White, ConsoleColor.Red);
                    continue;
                }

                Console.WriteLine("Is the applicant male or female (Valid values M/F)");
                Console.Write(">> ");
                string gender = (Console.ReadLine() ?? String.Empty).ToUpper();
                if (gender == "EXIT")
                {
                    return;
                }
                if (gender != "M" && gender != "F")
                {
                    ConsoleUtils.WriteLine("Invalid gender", ConsoleColor.White, ConsoleColor.Red);
                    continue;
                }
                gender = (gender == "M" ? "male" : "female");


                var messages = new List<ChatMessage>()
                {
                    new ChatMessage(ChatMessage.RoleEnum.User, $"The applicant is a {age} year old {gender} - are they eligible for insurance cover?  Please provide the reason for the decision.")
                };
                var request = new ChatCompletionRequest(ModelDefinitions.MistralSmall, messages);
                request.MaxTokens = 1024;
                request.Temperature = 0.0m;
                request.ToolChoice = ToolChoiceType.Auto;

                request.Tools = Mistral.SDK.Common.Tool.GetAllAvailableTools(includeDefaults: false, forceUpdate: true, clearCache: true).ToList();

                var response = await client.Completions.GetCompletionAsync(request).ConfigureAwait(false);

                messages.Add(response.Choices.First().Message);

                foreach (var toolCall in response.ToolCalls)
                {
                    var resp = await toolCall.InvokeAsync<string>();
                    messages.Add(new ChatMessage(toolCall, resp));
                }

                var response2 = await client.Completions.GetCompletionAsync(request).ConfigureAwait(false);
                ConsoleUtils.WriteLine(response2.Choices.First().Message.Content, ConsoleColor.Green);
            }

        }
    }
}
