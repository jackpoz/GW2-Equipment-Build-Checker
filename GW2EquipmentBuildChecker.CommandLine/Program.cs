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
            var api = new API(apiKey);

            // 1. Get the list of characters
            var characterNames = await api.GetCharactersNames();

            // 2. Pick 1 character
            foreach (var characterName in characterNames)
            {
                Console.WriteLine(characterName);
            }

            // 3. Get the list of builds

            // 4. Pick 1 build

            // 5. Get the list

            // 5. Get the build from gw2skills

            // 6. Compare and find differences
        }
    }
}
