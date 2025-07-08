using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2
{
    public class GW2API(string apiKey)
    {
        private const string BaseUrl = "https://api.guildwars2.com/v2";
        private HttpClient Client { get; } = new HttpClient();

        private static Entities.Specialization[] _specializations { get; set; }

        public async Task<string[]> GetCharactersNamesAsync()
        {
            string apiUrl = $"{BaseUrl}/characters";

            var contentResponse = await SendRequestAsync(apiUrl);
            var characterNames = JsonSerializer.Deserialize<string[]>(contentResponse) ?? Array.Empty<string>();

            return characterNames.Order().ToArray();
        }

        public async Task<BuildContainer[]> GetBuildsAsync(string selectedCharacterName)
        {
            string apiUrl = $"{BaseUrl}/characters/{EscapeCharacterName(selectedCharacterName)}/buildtabs?tabs=all";

            var contentResponse = await SendRequestAsync(apiUrl);
            var builds = JsonSerializer.Deserialize<BuildContainer[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<BuildContainer>();

            return builds;
        }

        public static async Task<string> GetSpecializationName(int? specializationId)
        {
            if (specializationId == null)
                return "<Not set>";

            var specialization = (await GetSpecializationsAsync()).First(s => s.Id == specializationId);
            return specialization.Name;
        }

        public static async Task<Entities.Specialization> GetSpecialization(int specializationId)
        {
            var specialization = (await GetSpecializationsAsync()).First(s => s.Id == specializationId);
            return specialization;
        }

        public static async Task<Entities.Specialization> GetSpecialization(string specializationName)
        {
            var specialization = (await GetSpecializationsAsync()).First(s => s.Name == specializationName);
            return specialization;
        }

        private static async Task<Entities.Specialization[]> GetSpecializationsAsync()
        {
            if (_specializations != null)
                return _specializations;

            using var client = new HttpClient();
            const string apiUrl = $"{BaseUrl}/specializations?ids=all";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var specializations = JsonSerializer.Deserialize<Entities.Specialization[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.Specialization>();

            _specializations = specializations;

            return specializations;
        }

        private async Task<string> SendRequestAsync(string apiUrl)
        {
            if (apiUrl.Contains('?'))
            {
                apiUrl += "&";
            }
            else
            {
                apiUrl += "?";
            }

            apiUrl += "access_token=" + apiKey;

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
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
