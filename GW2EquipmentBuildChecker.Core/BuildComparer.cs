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
        public static List<string> CompareBuilds(Build sourceBuild, Build targetBuild)
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
                    differences.Add($"Specialization mismatch: gw2skills has '{string.Join(", ", diffTargetSpecs.Select(s => GW2API.GetSpecializationName(s)))}', GW2 has '{string.Join(", ", diffSourceSpecs.Select(s => GW2API.GetSpecializationName(s)))}'");
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
                                var specData = GW2API.GetSpecialization(sourceSpec.Id.Value);
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
                                differences.Add($"Trait mismatch for specialization '{GW2API.GetSpecializationName(sourceSpec.Id)}': gw2skills has '{string.Join("-", targetTraitIndexes)}', GW2 has '{string.Join("-", sourceTraitIndexes)}'");
                                break;
                            }
                        }
                    }                    
                }
            }

            return differences;
        }
    }
}
