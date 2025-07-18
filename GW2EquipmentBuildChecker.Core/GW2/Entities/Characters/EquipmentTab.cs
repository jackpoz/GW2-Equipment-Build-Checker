using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2.Entities.Characters
{
    public class EquipmentContainer
    {
        public int Tab { get; set; }
        public string Name { get; set; }
        public bool Is_Active { get; set; }
        public List<Equipment> Equipment { get; set; } = new List<Equipment>();

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                Name = $"(Unnamed {Tab})";

            return $"{Tab}: {Name}";
        }
    }

    public class Equipment
    {
        public int Id { get; set; }
        public string Slot { get; set; }
        public List<int> Upgrades { get; set; } = new List<int>();
        public List<string> UpgradeNames { get; set; } = new List<string>();
        public List<int> Infusions { get; set; } = new List<int>();
        public List<string> InfusionNames { get; set; } = new List<string>();
        public EquipmentStats Stats { get; set; }
        public string Type { get; set; }
    }

    public class EquipmentStats
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
