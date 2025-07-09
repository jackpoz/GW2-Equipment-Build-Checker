using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2Skills.Entities
{
    internal class SkillsInfoContainer
    {
        public SkillContent[] Content { get; set; }
    }

    internal class SkillContent
    {
        public string[] Skill { get; set; }

        public string GetSkillName(int skillId)
        {
            var row = Skill.First(s => s.StartsWith($"i={skillId}#"));
            var startPrefix = "#n=";
            var startIndex = row.IndexOf(startPrefix) + startPrefix.Length;
            var endIndex = row.IndexOf("#d=", startIndex);
            return row.Substring(startIndex, endIndex - startIndex);
        }
    }
}
