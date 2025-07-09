using GW2EquipmentBuildChecker.Core.GW2;
using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using GW2EquipmentBuildChecker.Core.GW2Skills.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2Skills
{
    public class GW2SkillsAPI
    {
        private HttpClient Client { get; } = new HttpClient();
        private string Proxy { get; }

        public GW2SkillsAPI(string proxy = null)
        {
            Proxy = proxy;
        }

        public async Task<Build> GetBuildAsync(string buildUrl)
        {
            var contentResponse = await SendRequestAsync(buildUrl);

            const string sectionPrefix = "E = new BuildEditor(";
            var sectionBegin = contentResponse.IndexOf(sectionPrefix) + sectionPrefix.Length;
            var sectionEnd = contentResponse.IndexOf(");", sectionBegin + 1);
            var section = contentResponse.Substring(sectionBegin, sectionEnd - sectionBegin);

            var json = JavascriptToJson(section);
            var parsed = JsonSerializer.Deserialize<BuildAndEquipmentContainer>(json, JsonSerializerOptions.Web);

            var build = await ConvertGW2SkillsToGW2API(parsed);

            return build;
        }

        private async Task<string> SendRequestAsync(string url)
        {
            if (!string.IsNullOrEmpty(Proxy))
            {
                var uri = new Uri(url);
                url = $"{Proxy}{uri.PathAndQuery}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            return contentResponse;
        }

        private string JavascriptToJson(string javascriptCode)
        {
            var wip = Regex.Replace(javascriptCode, @"(?<={|,)\s*(\w+)\s*:", "\"$1\":");

            // Step 2: Replace single quotes with double quotes
            wip = Regex.Replace(wip, @"'", "\"");

            // Step 3: Remove trailing commas before closing braces/brackets
            wip = Regex.Replace(wip, @",\s*(?=[}\]])", "");

            wip = wip.Replace("SI || undefined", "true");

            return wip;
        }

        private async Task<SkillContent> GetSkillInfoAsync(int professionId)
        {
            var response = await Client.PostAsync("https://en.gw2skills.net/ajax/getSkillInfo/", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mode", "4"),
                new KeyValuePair<string, string>("id", professionId.ToString())
            }));

            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<SkillsInfoContainer>(contentResponse, JsonSerializerOptions.Web);

            return parsed.Content[0];
        }

        private async Task<Build> ConvertGW2SkillsToGW2API(BuildAndEquipmentContainer gw2SkillBuild)
        {
            var dbRaw = await SendRequestAsync($"https://en.gw2skills.net/ajax/db/en.{gw2SkillBuild.Dbid}.json");
            var db = JsonSerializer.Deserialize<Db>(dbRaw, JsonSerializerOptions.Web);

            var build = new Build()
            {
                Specializations = new List<GW2.Entities.Characters.Specialization>(),
                Skills = new SkillSet()
            };

            var profession = db.Profession.Rows.First(p => p[0].GetInt32() == gw2SkillBuild.Preload.Profession);
            build.Profession = profession[1].GetString();

            foreach (var trait in gw2SkillBuild.Preload.Trait)
            {
                var spec = db.Specialization.Rows.First(s => s[0].GetInt32() == trait[0]);
                int? trait1 = trait[1] == 0 ? null : spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[1]).Index;
                int? trait2 = trait[2] == 0 ? null : spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[2]).Index;
                int? trait3 = trait[3] == 0 ? null : spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[3]).Index;

                var gw2Spec = await GW2API.GetSpecialization(spec[1].GetString());
                var gw2Traits = new List<int?>() { trait1 == null ? null : gw2Spec.Major_Traits[trait1.Value], trait2 == null ? null : gw2Spec.Major_Traits[trait2.Value], trait3 == null ? null : gw2Spec.Major_Traits[trait3.Value] };

                build.Specializations.Add(new GW2.Entities.Characters.Specialization()
                {
                    Id = gw2Spec.Id,
                    Traits = gw2Traits
                });
            }

            var skillsInfo = await GetSkillInfoAsync(gw2SkillBuild.Preload.Profession);

            build.Skills.Heal = (await GW2API.GetSkill(skillsInfo.GetSkillName(gw2SkillBuild.Preload.Skill.T[0]["6"]))).Id;
            build.Skills.Utilities = new List<int?>()
            {
                 (await GW2API.GetSkill(skillsInfo.GetSkillName(gw2SkillBuild.Preload.Skill.T[0]["7"]))).Id,
                 (await GW2API.GetSkill(skillsInfo.GetSkillName(gw2SkillBuild.Preload.Skill.T[0]["8"]))).Id,
                 (await GW2API.GetSkill(skillsInfo.GetSkillName(gw2SkillBuild.Preload.Skill.T[0]["9"]))).Id
            };
            build.Skills.Elite = (await GW2API.GetSkill(skillsInfo.GetSkillName(gw2SkillBuild.Preload.Skill.T[0]["10"]))).Id;


            return build;
        }
    }
}
