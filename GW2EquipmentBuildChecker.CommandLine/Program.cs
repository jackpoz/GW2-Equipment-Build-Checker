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

            if (!int.TryParse(Console.ReadLine(), out var characterChoice) || characterChoice <= 0 || characterChoice > characterNames.Length)
            {
                Console.WriteLine("Invalid character choice");
                return;
            }

            var selectedCharacterName = characterNames[characterChoice - 1];

            // 3. Get the list of builds
            var builds = await gw2api.GetBuildsAsync(selectedCharacterName);

            // 4. Pick 1 build
            Console.WriteLine("\nBuilds list:");
            foreach (var build in builds)
            {
                Console.WriteLine(build.ToString());
            }

            Console.WriteLine("\nPick a build by writing the number:");

            if (!int.TryParse(Console.ReadLine(), out var buildChoice) || buildChoice <= 0 || buildChoice > builds.Length)
            {
                Console.WriteLine("Invalid build choice");
                return;
            }

            var selectedBuild = builds.First(b => b.Tab == buildChoice);

            // 5. Optionally select an equipment tab
            var equipments = await gw2api.GetEquipmentsAsync(selectedCharacterName);

            Console.WriteLine("\nEquipment list:");
            foreach (var equipment in equipments)
            {
                Console.WriteLine(equipment.ToString());
            }

            Console.WriteLine("\nPick an equipment by writing the number:");

            if (!int.TryParse(Console.ReadLine(), out var equipmentChoice) || equipmentChoice <= 0 || equipmentChoice > builds.Length)
            {
                Console.WriteLine("Invalid equipment choice");
                return;
            }

            var selectedEquipment = equipments.First(b => b.Tab == equipmentChoice);

            // 6. Get the build from gw2skills
            Console.WriteLine("\nPaste a gw2skills.net link:");
            var gw2skillsLink = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(gw2skillsLink))
            {
                Console.WriteLine("Invalid gw2skills link");
                return;
            }

            var gw2skills = new GW2SkillsAPI();

            var (gw2skillsBuild, gw2skillsEquipment) = await gw2skills.GetBuildAndEquipmentAsync(gw2skillsLink);

            // 7. Compare and find differences
            var buildDifferences = await BuildComparer.CompareBuildAndEquipment(selectedBuild.Build, gw2skillsBuild, selectedEquipment.Equipment, gw2skillsEquipment);

            // 8. Tell what to change
            if (buildDifferences.Count == 0)
            {
                Console.WriteLine("\nNo differences found");
            }
            else
            {
                Console.WriteLine();
                foreach (var difference in buildDifferences)
                {
                    Console.WriteLine(difference);
                }
            }
        }
    }
}
