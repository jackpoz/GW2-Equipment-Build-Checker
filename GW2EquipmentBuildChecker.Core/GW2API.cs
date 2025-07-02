using GW2EquipmentBuildChecker.Core.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core
{
    public class GW2API(string apiKey)
    {
        private const string BaseUrl = "https://api.guildwars2.com/v2";
        private HttpClient Client { get; } = new HttpClient();

        public async Task<string[]> GetCharactersNames()
        {
            string apiUrl = $"{BaseUrl}/characters";

            var contentResponse = await SendRequestAsync(apiUrl);
            var characterNames = JsonSerializer.Deserialize<string[]>(contentResponse) ?? Array.Empty<string>();

            return characterNames.Order().ToArray();
        }

        public async Task<BuildContainer[]> GetBuilds(string selectedCharacterName)
        {
            string apiUrl = $"{BaseUrl}/characters/{EscapeCharacterName(selectedCharacterName)}/buildtabs?tabs=all";

            var contentResponse = await SendRequestAsync(apiUrl);
            var builds = JsonSerializer.Deserialize<BuildContainer[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<BuildContainer>();

            return builds;
        }

        private async Task<string?> SendRequestAsync(string apiUrl)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            return contentResponse;
        }

        private string EscapeCharacterName(string characterName)
        {
            return Uri.EscapeDataString(characterName);
        }
    }
}
