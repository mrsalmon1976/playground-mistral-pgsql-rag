using Mistral.Config;
using Mistral.Examples;
using Mistral.Utils;

AppSettings appSettings = AppSettings.Load();

Console.WriteLine();
Console.WriteLine("Which example would you like to run?");
Console.WriteLine("\t[1] Embeddings: Local vector similarity search");
Console.WriteLine("\t[2] Chat context: Chat about new cricket law proposals");
Console.WriteLine("\t[3] Function execution: Ask an agent whether you are eligible for insurance cover based on custom rules");
Console.WriteLine();
ConsoleUtils.WriteLine("You can quit at any point by typing 'exit'.", ConsoleColor.Yellow);
Console.WriteLine();

string? exampleType = Console.ReadLine();

switch (exampleType)
{
    case "1":
        await LocalVectorSearch.Run(appSettings);
        break;
    case "2":
        await CricketLawChangeChat.Run(appSettings);
        break;
    case "3":
        await ExecuteFunction.Run(appSettings);
        break;
    default:
        ConsoleUtils.WriteLine("Invalid selection", ConsoleColor.White, ConsoleColor.Red);
        break;
}

Console.WriteLine("Press enter to exit");
Console.ReadLine();
