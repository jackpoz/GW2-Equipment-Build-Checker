using GW2EquipmentBuildChecker.Core;
using System;
using System.Configuration;

namespace GW2EquipmentBuildChecker.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var apiKey = ConfigurationManager.AppSettings["ApiKey"] ?? throw new ArgumentNullException("ApiKey");
            var api = new GW2API(apiKey);

            // 1. Get the list of characters
            var characterNames = await api.GetCharactersNames();

            // 2. Pick 1 character
            Console.WriteLine("Characters list:");
            var characterIndex = 0;
            foreach (var characterName in characterNames)
            {
                Console.WriteLine($"{characterIndex + 1}: {characterName}");
                ++characterIndex;
            }

            Console.WriteLine("\nPick a character by writing the number...");

            if (!int.TryParse(Console.ReadLine(), out var characterChoice) && characterChoice > 0 && characterChoice <= characterNames.Length)
            {
                Console.WriteLine("Invalid character choice");
                return;
            }

            var selectedCharacterName = characterNames[characterChoice - 1];

            // 3. Get the list of builds
            var builds = await api.GetBuilds(selectedCharacterName);

            // 4. Pick 1 build
            Console.WriteLine("Builds list:");
            foreach (var build in builds)
            {
                if (string.IsNullOrEmpty(build.Build.Name))
                    build.Build.Name = $"(Unnamed {build.Tab})";

                Console.WriteLine($"{build.Tab}: {build.Build.Name}");
            }

            Console.WriteLine("\nPick a build by writing the number...");

            if (!int.TryParse(Console.ReadLine(), out var buildChoice) && buildChoice > 0 && buildChoice <= builds.Length)
            {
                Console.WriteLine("Invalid build choice");
                return;
            }

            var selectedBuild = builds.Where(b => b.Tab == buildChoice).First();

            // 5. Get the build from gw2skills

            // 6. Compare and find differences

            // 7. Tell what to change
        }
    }
}
