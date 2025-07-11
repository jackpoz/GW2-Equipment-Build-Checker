﻿using GW2EquipmentBuildChecker.Core.GW2;
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
        public static async Task<List<string>> CompareBuilds(Build sourceBuild, Build targetBuild)
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
                        var targetSpec = targetBuild.Specializations.First(s => s.Id == sourceSpec.Id);

                        for (int traitIndex = 0; traitIndex < 3; traitIndex++)
                        {
                            if (sourceSpec.Traits[traitIndex] != targetSpec.Traits[traitIndex])
                            {
                                var specData = await GW2API.GetSpecialization(sourceSpec.Id.Value);
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
                            differences.Add($"Heal skill mismatch: gw2skills has '{await GW2API.GetSkillName(targetBuild.Skills.Heal)}', GW2 has '{await GW2API.GetSkillName(sourceBuild.Skills.Heal)}'");
                        }

                        var diffSourceSkills = sourceBuild.Skills.Utilities.Except(targetBuild.Skills.Utilities);
                        var diffTargetSkills = targetBuild.Skills.Utilities.Except(sourceBuild.Skills.Utilities);
                        if (diffSourceSkills.Any() || diffTargetSkills.Any())
                        {
                            differences.Add($"Utilities skills mismatch: gw2skills has '{string.Join(", ", await Task.WhenAll(diffTargetSkills.Select(async s => await GW2API.GetSkillName(s))))}', GW2 has '{string.Join(", ", await Task.WhenAll(diffSourceSkills.Select(async s => await GW2API.GetSkillName(s))))}'");
                        }

                        if (sourceBuild.Skills.Elite != targetBuild.Skills.Elite)
                        {
                            differences.Add($"Elite skill mismatch: gw2skills has '{await GW2API.GetSkillName(targetBuild.Skills.Elite)}', GW2 has '{await GW2API.GetSkillName(sourceBuild.Skills.Elite)}'");
                        }
                    }
                }
            }

            return differences;
        }
    }
}
