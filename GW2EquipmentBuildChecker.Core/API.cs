using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core
{
    public class API(string apiKey)
    {
        private const string BaseUrl = "https://api.guildwars2.com/v2/";
        private HttpClient Client { get; } = new HttpClient();

        public async Task<string[]> GetCharactersNames()
        {
            const string apiUrl = $"{BaseUrl}characters";

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var characterNames = JsonSerializer.Deserialize<string[]>(contentResponse) ?? Array.Empty<string>();

            return characterNames;
        }
    }
}
