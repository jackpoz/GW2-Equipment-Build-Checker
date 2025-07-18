using GW2EquipmentBuildChecker.Core.GW2;
using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core
{
    public static class BuildComparer
    {
        public static async Task<List<string>> CompareBuildAndEquipment(Build sourceBuild, Build targetBuild, List<GW2.Entities.Characters.Equipment> sourceEquipment, List<GW2.Entities.Characters.Equipment> targetEquipment)
        {
            var differences = new List<string>();
            if (sourceBuild.Profession != targetBuild.Profession)
            {
                differences.Add($"Profession mismatch: gw2skills has '{targetBuild.Profession}', GW2 has '{sourceBuild.Profession}'");
            }
            else
            {
                var sourceSpecs = sourceBuild.Specializations.Select(s => s.Id);
                var targetSpecs = targetBuild.Specializations.Select(s => s.Id);
                var diffSourceSpecs = sourceSpecs.Except(targetSpecs);
                var diffTargetSpecs = targetSpecs.Except(sourceSpecs);

                if (diffSourceSpecs.Any() || diffTargetSpecs.Any())
                {
                    differences.Add($"Specialization mismatch: gw2skills has '{string.Join(", ", await Task.WhenAll(diffTargetSpecs.Select(async s => await GW2API.GetSpecializationName(s))))}', GW2 has '{string.Join(", ", await Task.WhenAll(diffSourceSpecs.Select(async s => await GW2API.GetSpecializationName(s))))}'");
                }
                else
                {
                    foreach(var sourceSpec in sourceBuild.Specializations)
                    {
                        var targetSpec = targetBuild.Specializations.Single(s => s.Id == sourceSpec.Id);

                        for (int traitIndex = 0; traitIndex < 3; traitIndex++)
                        {
                            if (sourceSpec.Traits[traitIndex] != targetSpec.Traits[traitIndex])
                            {
                                var specData = await GW2API.GetSpecializationById(sourceSpec.Id.Value);
                                var targetTraitIndexes = new[] {
                                    specData.GetMajorTraitIndex(targetSpec.Traits[0]) +1,
                                    specData.GetMajorTraitIndex(targetSpec.Traits[1]) +1,
                                    specData.GetMajorTraitIndex(targetSpec.Traits[2]) +1,
                                };
                                var sourceTraitIndexes = new[] {
                                    specData.GetMajorTraitIndex(sourceSpec.Traits[0]) +1,
                                    specData.GetMajorTraitIndex(sourceSpec.Traits[1]) +1,
                                    specData.GetMajorTraitIndex(sourceSpec.Traits[2]) +1,
                                };
                                differences.Add($"Trait mismatch for specialization '{await GW2API.GetSpecializationName(sourceSpec.Id)}': gw2skills has '{string.Join("-", targetTraitIndexes)}', GW2 has '{string.Join("-", sourceTraitIndexes)}'");
                                break;
                            }
                        }
                    }

                    // Revenants have skills 6-10 set by their legends
                    if (sourceBuild.Profession == "Revenant")
                    {
                        var diffSourceLegends = sourceBuild.Legends.Except(targetBuild.Legends);
                        var diffTargetLegends = targetBuild.Legends.Except(sourceBuild.Legends);
                        if (diffSourceLegends.Any() || diffTargetLegends.Any())
                        {
                            differences.Add($"Legends mismatch: gw2skills has '{string.Join(", ", await Task.WhenAll(diffTargetLegends.Select(async l => (await GW2API.GetLegendById(l)).Name)))}', GW2 has '{string.Join(", ", await Task.WhenAll(diffSourceLegends.Select(async l => (await GW2API.GetLegendById(l)).Name)))}'");
                        }
                    }
                    else
                    {
                        if (sourceBuild.Skills.Heal != targetBuild.Skills.Heal)
                        {
                            differences.Add($"Heal skill mismatch: gw2skills has '{await GW2API.GetSkillNameById(targetBuild.Skills.Heal)}', GW2 has '{await GW2API.GetSkillNameById(sourceBuild.Skills.Heal)}'");
                        }

                        var diffSourceSkills = sourceBuild.Skills.Utilities.Except(targetBuild.Skills.Utilities);
                        var diffTargetSkills = targetBuild.Skills.Utilities.Except(sourceBuild.Skills.Utilities);
                        if (diffSourceSkills.Any() || diffTargetSkills.Any())
                        {
                            differences.Add($"Utilities skills mismatch: gw2skills has '{string.Join(", ", await Task.WhenAll(diffTargetSkills.Select(async s => await GW2API.GetSkillNameById(s))))}', GW2 has '{string.Join(", ", await Task.WhenAll(diffSourceSkills.Select(async s => await GW2API.GetSkillNameById(s))))}'");
                        }

                        if (sourceBuild.Skills.Elite != targetBuild.Skills.Elite)
                        {
                            differences.Add($"Elite skill mismatch: gw2skills has '{await GW2API.GetSkillNameById(targetBuild.Skills.Elite)}', GW2 has '{await GW2API.GetSkillNameById(sourceBuild.Skills.Elite)}'");
                        }
                    }
                }
            }

            if (sourceEquipment != null && targetEquipment != null)
            {
                // Process weapons separately as they could be swapped
                var sourceEquipmentBySlot = sourceEquipment.Where(e => !e.Slot.StartsWith("Weapon")).ToDictionary(e => e.Slot, e => e);
                var targetEquipmentBySlot = targetEquipment.Where(e => !e.Slot.StartsWith("Weapon")).ToDictionary(e => e.Slot, e => e);
                CompareEquipment(differences, sourceEquipmentBySlot, targetEquipmentBySlot);

                var sourceWeapons = sourceEquipment.Where(e => e.Slot.StartsWith("Weapon"));
                var targetWeapons = targetEquipment.Where(e => e.Slot.StartsWith("Weapon"));

                // Check if weapon slots are swapped and swap them back
                if (sourceWeapons.Select(w => w.Slot).Except(targetWeapons.Select(w => w.Slot)).Any()
                    || sourceWeapons.Select(w => new { w.Slot, w.Type }).Except(targetWeapons.Select(w => new { w.Slot, w.Type })).Any())
                {
                    SwapWeaponsSlot(sourceWeapons, 1);
                    SwapWeaponsSlot(sourceWeapons, 2);
                }

                // Check if weapon sigils are swapped and swap them back
                CompareAndSwapSigils(sourceWeapons, targetWeapons);

                var sourceWeaponsBySlot = sourceWeapons.ToDictionary(e => e.Slot, e => e);
                var targetWeaponsBySlot = targetWeapons.ToDictionary(e => e.Slot, e => e);

                CompareEquipment(differences, sourceWeaponsBySlot, targetWeaponsBySlot);

                differences.Add("Disclaimer: relics and non-legendary items with selectable stats cannot be compared due to GW2 API limitations.");
            }

            return differences;
        }

        private static void CompareEquipment(List<string> differences, Dictionary<string, Equipment> sourceEquipmentBySlot, Dictionary<string, Equipment> targetEquipmentBySlot)
        {
            foreach (var slot in sourceEquipmentBySlot.Keys.Union(targetEquipmentBySlot.Keys))
            {
                if (!sourceEquipmentBySlot.TryGetValue(slot, out var sourceItem))
                {
                    differences.Add($"Missing equipment in slot '{slot}' in GW2");
                }
                else if (!targetEquipmentBySlot.TryGetValue(slot, out var targetItem))
                {
                    // No need to log missing equipment in gw2skills
                }
                else
                {
                    if (sourceItem.Stats?.Name != targetItem.Stats.Name)
                    {
                        differences.Add($"Stats mismatch in slot '{slot}{(slot.StartsWith("Weapon") ? " (" + targetItem.Type + ")" : "")}': gw2skills has '{targetItem.Stats.Name}', GW2 has '{sourceItem.Stats?.Name}'");
                    }

                    if (slot.StartsWith("Weapon") && sourceItem.Type != targetItem.Type)
                    {
                        differences.Add($"Weapon type mismatch in slot '{slot}': gw2skills has '{targetItem.Type}', GW2 has '{sourceItem.Type}'");
                    }

                    var diffSourceUpgrades = sourceItem.UpgradeNames.Except(targetItem.UpgradeNames);
                    var diffTargetUpgrades = targetItem.UpgradeNames.Except(sourceItem.UpgradeNames);
                    if (diffSourceUpgrades.Any() || diffTargetUpgrades.Any())
                    {
                        differences.Add($"Upgrade mismatch in slot '{slot}{(slot.StartsWith("Weapon") ? " (" + targetItem.Type + ")" : "")}': gw2skills has '{string.Join(", ", diffTargetUpgrades)}', GW2 has '{string.Join(", ", diffSourceUpgrades)}'");
                    }
                }
            }
        }

        private static void SwapWeaponsSlot(IEnumerable<Equipment> weapons, int slot)
        {
            var weaponA = weapons.SingleOrDefault(w => w.Slot == $"WeaponA{slot}");
            var weaponB = weapons.SingleOrDefault(w => w.Slot == $"WeaponB{slot}");

            if (weaponA != null)
            {
                weaponA.Slot = $"WeaponB{slot}";
            }

            if (weaponB != null)
            {
                weaponB.Slot = $"WeaponA{slot}";
            }
        }

        private static void CompareAndSwapSigils(IEnumerable<Equipment> sourceWeapons, IEnumerable<Equipment> targetWeapons)
        {
            foreach (var sourceWeapon in sourceWeapons)
            {
                var targetWeapon = targetWeapons.SingleOrDefault(tw => tw.Slot == sourceWeapon.Slot && !tw.UpgradeNames.Order().SequenceEqual(sourceWeapon.UpgradeNames.Order()));
                if (targetWeapon != null)
                {
                    // Get the other weapon in the same set if any
                    var otherSourceWeapon = sourceWeapons.SingleOrDefault(sw => sw.Slot == $"{sourceWeapon.Slot.Substring(0, sourceWeapon.Slot.Length - 1)}{(sourceWeapon.Slot[^1] == '1' ? "2" : "1")}");

                    // Swap sigils if the other weapon has the target one
                    if (otherSourceWeapon != null && otherSourceWeapon.UpgradeNames.Order().SequenceEqual(targetWeapon.UpgradeNames.Order()))
                    {
                        var sourceUpgrades = sourceWeapon.Upgrades;
                        var sourceUpgradeNames = sourceWeapon.UpgradeNames;

                        sourceWeapon.Upgrades = otherSourceWeapon.Upgrades;
                        sourceWeapon.UpgradeNames = otherSourceWeapon.UpgradeNames;

                        otherSourceWeapon.Upgrades = sourceUpgrades;
                        otherSourceWeapon.UpgradeNames = sourceUpgradeNames;
                    }
                }
            }
        }
    }
}
