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

        public static Entities.Specialization[] Specializations => _specializations.Value;
        private static readonly Lazy<Entities.Specialization[]> _specializations = new(() =>
        {
            using var client = new HttpClient();
            return GetSpecializationsAsync(client).GetAwaiter().GetResult();
        });

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

        public static string GetSpecializationName(int? specializationId)
        {
            if (specializationId == null)
                return "<Not set>";

            var specialization = Specializations.First(s => s.Id == specializationId);
            return specialization.Name;
        }

        public static Entities.Specialization GetSpecialization(int specializationId)
        {
            var specialization = Specializations.First(s => s.Id == specializationId);
            return specialization;
        }

        public static Entities.Specialization GetSpecialization(string specializationName)
        {
            var specialization = Specializations.First(s => s.Name == specializationName);
            return specialization;
        }

        private static async Task<Entities.Specialization[]> GetSpecializationsAsync(HttpClient client)
        {
            const string apiUrl = $"{BaseUrl}/specializations?ids=all";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var specializations = JsonSerializer.Deserialize<Entities.Specialization[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.Specialization>();
            return specializations;
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
