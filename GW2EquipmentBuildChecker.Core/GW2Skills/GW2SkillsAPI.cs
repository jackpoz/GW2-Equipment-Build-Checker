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
    public class GW2SkillsAPI()
    {
        private HttpClient Client { get; } = new HttpClient();

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

        private async Task<string?> SendRequestAsync(string url)
        {
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

        private async Task<Build> ConvertGW2SkillsToGW2API(BuildAndEquipmentContainer gw2SkillBuild)
        {
            var dbRaw = await SendRequestAsync($"https://en.gw2skills.net/ajax/db/en.{gw2SkillBuild.Dbid}.json");
            var db = JsonSerializer.Deserialize<Db>(dbRaw, JsonSerializerOptions.Web);

            var gw2Specializations = await GW2API.GetSpecializationsAsync();

            var build = new Build()
            {
                Specializations = new List<GW2.Entities.Characters.Specialization>()
            };

            foreach(var trait in gw2SkillBuild.Preload.Trait)
            {
                var spec = db.Specialization.Rows.Single(s => s[0].GetInt32() == trait[0]);
                var trait1 = spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[1]).Index;
                var trait2 = spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[2]).Index;
                var trait3 = spec[7].EnumerateArray().Index().First(t => t.Item.GetInt32() == trait[3]).Index;

                var gw2Spec = gw2Specializations.First(s => s.Name == spec[1].GetString());
                var gw2Traits = new List<int?>() { gw2Spec.Major_Traits[trait1], gw2Spec.Major_Traits[trait2], gw2Spec.Major_Traits[trait3] };

                build.Specializations.Add(new GW2.Entities.Characters.Specialization()
                {
                    Id = gw2Spec.Id,
                    Traits = gw2Traits
                });
            }

            return build;
        }
    }
}
