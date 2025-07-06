using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2.Entities.Characters
{
    public class BuildContainer
    {
        public int Tab { get; set; }
        public bool Is_Active { get; set; }
        public Build Build { get; set; }

        public override string ToString()
        {
            var name = Build?.Name?.ToString();
            if (string.IsNullOrEmpty(name))
                name = $"(Unnamed {Tab})";

            return $"{Tab}: {name}";
        }
    }

    public class Build
    {
        public string Name { get; set; }
        public string Profession { get; set; }
        public List<Specialization> Specializations { get; set; }
        public SkillSet Skills { get; set; }
        public SkillSet Aquatic_Skills { get; set; }
        public List<string?> Legends { get; set; }
        public List<string> Aquatic_Legends { get; set; }
    }

    public class Specialization
    {
        public int? Id { get; set; }
        public List<int?> Traits { get; set; }
    }

    public class SkillSet
    {
        public int? Heal { get; set; }
        public List<int?> Utilities { get; set; }
        public int? Elite { get; set; }
    }
}
