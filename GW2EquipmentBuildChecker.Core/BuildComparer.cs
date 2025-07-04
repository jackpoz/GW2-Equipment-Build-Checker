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

                var gw2Specializations = await GW2API.GetSpecializationsAsync();

                if (diffSourceSpecs.Any() || diffTargetSpecs.Any())
                {
                    differences.Add($"Specialization mismatch: gw2skills has '{string.Join(", ", diffTargetSpecs.Select(s => gw2Specializations.First(ss => ss.Id == s).Name))}', GW2 has '{string.Join(", ", diffSourceSpecs.Select(s => gw2Specializations.First(ss => ss.Id == s).Name))}'");
                }
                else
                {
                }
            }

            return differences;
        }
    }
}
