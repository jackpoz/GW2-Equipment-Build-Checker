using GW2EquipmentBuildChecker.Core;
using GW2EquipmentBuildChecker.Core.GW2;
using GW2EquipmentBuildChecker.Core.GW2Skills;
using System;
using System.Configuration;

namespace GW2EquipmentBuildChecker.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var apiKey = config.AppSettings.Settings["ApiKey"].Value;
            while (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("Please enter your API key:");
                apiKey = Console.ReadLine()?.Trim() ?? string.Empty;
                config.AppSettings.Settings["ApiKey"].Value = apiKey;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }

            var gw2api = new GW2API(apiKey);

            // 1. Get the list of characters
            var characterNames = await gw2api.GetCharactersNamesAsync();

            // 2. Pick 1 character
            Console.WriteLine("Characters list:");
            var characterIndex = 0;
            foreach (var characterName in characterNames)
            {
                Console.WriteLine($"{characterIndex + 1}: {characterName}");
                ++characterIndex;
            }

            Console.WriteLine("\nPick a character by writing the number:");

            if (!int.TryParse(Console.ReadLine(), out var characterChoice) && characterChoice > 0 && characterChoice <= characterNames.Length)
            {
                Console.WriteLine("Invalid character choice");
                return;
            }

            var selectedCharacterName = characterNames[characterChoice - 1];

            // 3. Get the list of builds
            var builds = await gw2api.GetBuildsAsync(selectedCharacterName);

            // 4. Pick 1 build
            Console.WriteLine("Builds list:");
            foreach (var build in builds)
            {
                if (string.IsNullOrEmpty(build.Build.Name))
                    build.Build.Name = $"(Unnamed {build.Tab})";

                Console.WriteLine($"{build.Tab}: {build.Build.Name}");
            }

            Console.WriteLine("\nPick a build by writing the number:");

            if (!int.TryParse(Console.ReadLine(), out var buildChoice) && buildChoice > 0 && buildChoice <= builds.Length)
            {
                Console.WriteLine("Invalid build choice");
                return;
            }

            var selectedBuild = builds.First(b => b.Tab == buildChoice);

            // 5. Get the build from gw2skills
            Console.WriteLine("Past a gw2skills.net link:");
            var gw2skillsLink = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(gw2skillsLink))
            {
                Console.WriteLine("Invalid gw2skills link");
                return;
            }

            var gw2skills = new GW2SkillsAPI();

            var gw2skillsBuild = await gw2skills.GetBuildAsync(gw2skillsLink);

            // 6. Compare and find differences
            var buildDifferences = BuildComparer.CompareBuilds(selectedBuild.Build, gw2skillsBuild);

            // 7. Tell what to change
            foreach (var difference in buildDifferences)
            {
                Console.WriteLine(difference);
            }
        }
    }
}
