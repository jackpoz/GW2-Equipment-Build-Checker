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
                differences.Add($"Profession mismatch: GW2 has '{sourceBuild.Profession}' , gw2skills has '{targetBuild.Profession}'");
            }
            else
            {

            }

            return differences;
        }
    }
}
