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
        private static Entities.Skill[] _skills { get; set; }
        private static Entities.Legend[] _legends { get; set; }

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
            var buildContainers = JsonSerializer.Deserialize<BuildContainer[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<BuildContainer>();

            // Map the legendaries as GW2 API is buggy
            if (buildContainers.First().Build.Profession == "Revenant")
            {
                foreach (var buildContainer in buildContainers)
                {
                    for (int buildIndex = 0; buildIndex < buildContainer.Build.Legends.Count; buildIndex++)
                    {
                        buildContainer.Build.Legends[buildIndex] = await MapLegendaryFromBuild(buildContainer.Build.Legends[buildIndex], buildContainer.Build.Specializations);
                    }
                }
            }

            return buildContainers;
        }

        private async Task<string> MapLegendaryFromBuild(string legendary, List<Specialization> specializations)
        {
            switch (legendary)
            {
                case "Fire":
                    return "Legend1";
                case "Water":
                    return "Legend2";
                case "Air":
                    return "Legend3";
                case "Earth":
                    return "Legend4";
                case "Deathshroud":
                    return "Legend6";
                case null:
                    {
                        switch (await GetSpecializationName(specializations.Last().Id))
                        {
                            case "Renegade":
                                return "Legend5";
                            case "Vindicator":
                                return "Legend7";
                            default:
                                return null;
                        }
                    }
                default:
                    return legendary;
            }
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

        public static async Task<string> GetSkillName(int? skillId)
        {
            if (skillId == null)
                return "<Not set>";

            var skill = (await GetSkillsAsync()).First(s => s.Id == skillId);
            return skill.Name;
        }

        public static async Task<Entities.Skill> GetSkill(string skillName)
        {
            var skill = (await GetSkillsAsync()).First(s => s.Name == skillName);
            return skill;
        }

        private static async Task<Entities.Skill[]> GetSkillsAsync()
        {
            if (_skills != null)
                return _skills;

            using var client = new HttpClient();
            const string apiUrl = $"{BaseUrl}/skills?ids=all";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var skills = JsonSerializer.Deserialize<Entities.Skill[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.Skill>();

            _skills = skills;

            return skills;
        }

        public static async Task<Entities.Legend> GetLegendById(string legendId)
        {
            var legends = await GetLegendsAsync();
            return legends.First(l => l.Id == legendId);
        }

        public static async Task<Entities.Legend> GetLegendBySkills(int healSkillId, int eliteSkillId)
        {
            var legends = await GetLegendsAsync();
            // Hope for that best that at least the heal or the elite skill is set
            return legends.First(l => l.Heal == healSkillId || l.Elite == eliteSkillId);
        }

        public static async Task<Entities.Legend[]> GetLegendsAsync()
        {
            if (_legends != null)
                return _legends;

            using var client = new HttpClient();
            const string apiUrl = $"{BaseUrl}/legends?ids=all";
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var legends = JsonSerializer.Deserialize<Entities.Legend[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.Legend>();

            foreach (var legend in legends)
            {
                legend.Name = legend.Id switch
                {
                    "Legend1" => "Glint",
                    "Legend2" => "Shiro",
                    "Legend3" => "Jalis",
                    "Legend4" => "Mallyx",
                    "Legend5" => "Renegade",
                    "Legend6" => "Ventari",
                    _ => legend.Id
                };
            }

            // Add missing legends. Vindicator has 2 sets of skills
            legends = legends.Append(new Entities.Legend
            {
                Id = "Legend7",
                Name = "Vindicator",
                Heal = 62719,
                Elite = 62942
            }).Append(new Entities.Legend
            {
                Id = "Legend7",
                Name = "Vindicator",
                Heal = 62680,
                Elite = 62687
            }).ToArray();

            _legends = legends;

            return legends;
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
