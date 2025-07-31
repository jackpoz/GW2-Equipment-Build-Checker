using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using System;
using System.Collections.Concurrent;
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
        private static HttpClient Client { get; } = new HttpClient();

        private static Entities.Specialization[] _specializations { get; set; }
        private static Entities.Skill[] _skills { get; set; }
        private static Entities.Legend[] _legends { get; set; }
        private static Entities.ItemStat[] _itemStats { get; set; }

        private static ConcurrentDictionary<int, Entities.Item> _itemsCache { get; } = new ConcurrentDictionary<int, Entities.Item>();

        public async Task<string[]> GetCharactersNamesAsync()
        {
            string apiUrl = $"{BaseUrl}/characters";

            var contentResponse = await SendRequestAsync(apiUrl);
            var characterNames = JsonSerializer.Deserialize<string[]>(contentResponse) ?? Array.Empty<string>();

            return [..characterNames.Order()];
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

        public async Task<EquipmentTab[]> GetEquipmentsAsync(string selectedCharacterName)
        {
            string apiUrl = $"{BaseUrl}/characters/{EscapeCharacterName(selectedCharacterName)}/equipmenttabs?tabs=all";
            var contentResponse = await SendRequestAsync(apiUrl);
            var equipmentTabs = JsonSerializer.Deserialize<EquipmentTab[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<EquipmentTab>();

            var itemIds = equipmentTabs.SelectMany(et => et.Equipment.SelectMany(e => new[] { e.Id }.Concat(e.Upgrades).Concat(e.Infusions))).Distinct();
            await GetItemsByIds(itemIds);

            // Skip aquatic weapons as they are not so relevant
            foreach (var equipmentContainer in equipmentTabs)
            {
                equipmentContainer.Equipment.RemoveAll(ec => ec.Slot.Contains("Aquatic"));
                foreach (var equipment in equipmentContainer.Equipment)
                {
                    if (equipment.Stats != null)
                    {
                        equipment.Stats.Name = (await GetItemStatById(equipment.Stats.Id)).Name;
                    }
                    else
                    {
                        // Get the stats name from the item itself
                        var item = await GetItemById(equipment.Id);
                        if (item.Details?.Infix_Upgrade?.Id != null)
                        {
                            var itemStat = await GetItemStatById(item.Details.Infix_Upgrade.Id);
                            equipment.Stats = new EquipmentStats
                            {
                                Id = itemStat.Id,
                                Name = itemStat.Name
                            };
                        }
                    }

                    if (equipment.Slot.StartsWith("Weapon"))
                    {
                        var item = await GetItemById(equipment.Id);
                        equipment.Type = item.Details.Type;

                        if (equipment.Type == "Harpoon")
                        {
                            equipment.Type = "Spear";
                        }
                    }
                    else
                    {
                        equipment.Type = "(Gear)";
                    }

                    equipment.UpgradeNames = [.. (await Task.WhenAll(equipment.Upgrades.Select(async itemId =>
                    {
                        var item = await GetItemById(itemId);
                        return item.Name;
                    })))];

                    equipment.InfusionNames = [.. (await Task.WhenAll(equipment.Infusions.Select(async itemId =>
                    {
                        var item = await GetItemById(itemId);
                        return item.Name;
                    })))];
                }

                if (equipmentContainer.Is_Active)
                {
                    // Get the relic from the active equipment
                    var apiUrlEquip = $"{BaseUrl}/characters/{EscapeCharacterName(selectedCharacterName)}/equipment";
                    var contentResponseEquip = await SendRequestAsync(apiUrlEquip);
                    var activeEquipmentTab = JsonSerializer.Deserialize<EquipmentTab>(contentResponseEquip, JsonSerializerOptions.Web);
                    var relic = activeEquipmentTab?.Equipment.SingleOrDefault(e => e.Slot == "Relic");
                    if (relic != null)
                    {
                        var relicItem = await GetItemById(relic.Id);
                        equipmentContainer.Equipment.Add(new Equipment
                        {
                            Id = relic.Id,
                            Name = relicItem.Name,
                            Slot = relic.Slot,
                            Type = "(Gear)"
                        });
                    }
                }
            }

            return equipmentTabs;
        }

        public static async Task<string> GetSpecializationName(int? specializationId)
        {
            if (specializationId == null)
                return "<Not set>";

            var specialization = (await GetSpecializationsAsync()).First(s => s.Id == specializationId);
            return specialization.Name;
        }

        public static async Task<Entities.Specialization> GetSpecializationById(int specializationId)
        {
            var specialization = (await GetSpecializationsAsync()).First(s => s.Id == specializationId);
            return specialization;
        }

        public static async Task<Entities.Specialization> GetSpecializationByName(string specializationName)
        {
            var specialization = (await GetSpecializationsAsync()).First(s => s.Name == specializationName);
            return specialization;
        }

        private static async Task<Entities.Specialization[]> GetSpecializationsAsync()
        {
            if (_specializations != null)
                return _specializations;

            const string apiUrl = $"{BaseUrl}/specializations?ids=all&lang=en";
            var response = await Client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var specializations = JsonSerializer.Deserialize<Entities.Specialization[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.Specialization>();

            _specializations = specializations;

            return specializations;
        }

        public static async Task<string> GetSkillNameById(int? skillId)
        {
            if (skillId == null)
                return "<Not set>";

            var skill = (await GetSkillsAsync()).First(s => s.Id == skillId);
            return skill.Name;
        }

        public static async Task<Entities.Skill> GetSkillByName(string skillName)
        {
            var skill = (await GetSkillsAsync()).First(s => s.Name == skillName);
            return skill;
        }

        private static async Task<Entities.Skill[]> GetSkillsAsync()
        {
            if (_skills != null)
                return _skills;

            const string apiUrl = $"{BaseUrl}/skills?ids=all&lang=en";
            var response = await Client.GetAsync(apiUrl);
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

            const string apiUrl = $"{BaseUrl}/legends?ids=all";
            var response = await Client.GetAsync(apiUrl);
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
            legends = [..legends.Append(new Entities.Legend
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
            })];

            _legends = legends;

            return legends;
        }

        public static async Task<Entities.ItemStat[]> GetItemStatsAsync()
        {
            if (_itemStats != null)
                return _itemStats;

            const string apiUrl = $"{BaseUrl}/itemstats?ids=all&lang=en";
            var response = await Client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var contentResponse = await response.Content.ReadAsStringAsync();
            var itemStats = JsonSerializer.Deserialize<Entities.ItemStat[]>(contentResponse, JsonSerializerOptions.Web) ?? Array.Empty<Entities.ItemStat>();

            foreach (var itemStat in itemStats)
            {
                itemStat.Name = itemStat.Name.Replace("'s", "");
                if (string.IsNullOrEmpty(itemStat.Name))
                {
                    itemStat.Name = "(None)";
                }
            }

            _itemStats = itemStats;

            return itemStats;
        }

        public static async Task<Entities.ItemStat> GetItemStatById(int itemStatId)
        {
            var itemStats = await GetItemStatsAsync();
            return itemStats.First(s => s.Id == itemStatId);
        }

        public static async Task<Entities.Item> GetItemById(int itemId)
        {
            return (await GetItemsByIds([itemId])).Single();
        }

        public static async Task<Entities.Item[]> GetItemsByIds(IEnumerable<int> itemIds)
        {
            var items = new List<Entities.Item>();
            var itemIdsToFetch = new List<int>();

            // Try to get items from the cache first
            foreach (var itemId in itemIds)
            {
                if (_itemsCache.TryGetValue(itemId, out var cachedItem))
                {
                    items.Add(cachedItem);
                }
                else
                {
                    itemIdsToFetch.Add(itemId);
                }
            }

            // Fetch items in bulk from GW2 API if not cached
            if (itemIdsToFetch.Count != 0)
            {
                try
                {
                    string apiUrl = $"{BaseUrl}/items?ids={string.Join(",", itemIdsToFetch)}&lang=en";
                    var response = await Client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    var contentResponse = await response.Content.ReadAsStringAsync();
                    var itemsFetched = JsonSerializer.Deserialize<Entities.Item[]>(contentResponse, JsonSerializerOptions.Web);

                    foreach (var item in itemsFetched)
                    {
                        _itemsCache.TryAdd(item.Id, item);
                        items.Add(item);
                    }
                }
                catch (Exception)
                {
                    foreach (var itemId in itemIdsToFetch)
                    {
                        var item = new Entities.Item
                        {
                            Id = itemId,
                            Name = $"(Item {itemId} not found)",
                            Details = new Entities.ItemDetails
                            {
                                Type = "Unknown"
                            }
                        };

                        _itemsCache.TryAdd(itemId, item);
                        items.Add(item);
                    }
                }
            }

            return [.. items];
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

            apiUrl += "access_token=" + apiKey + "&nocache=" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            var response = await Client.SendAsync(request);
            await EnsureSuccessRequest(response);
            var contentResponse = await response.Content.ReadAsStringAsync();
            return contentResponse;
        }

        private async Task EnsureSuccessRequest(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = null;
                try
                {
                    var contentRaw = await response.Content.ReadAsStringAsync();
                    var content = JsonSerializer.Deserialize<Dictionary<string, string>>(contentRaw);
                    errorMessage = content["text"];
                }
                catch
                {
                    response.EnsureSuccessStatusCode();
                }

                throw new Exception($"GW2 API error: '{errorMessage}'");
            }
        }

        private string EscapeCharacterName(string characterName)
        {
            return Uri.EscapeDataString(characterName);
        }
    }
}
