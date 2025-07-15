using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2Skills.Entities
{
    internal class BuildAndEquipmentContainer
    {
        public string Version { get; set; }
        public string Balance { get; set; }
        public long Dbid { get; set; }
        public bool Showinfo { get; set; }
        public Preload Preload { get; set; }
    }

    internal class Preload
    {
        public string Qlink { get; set; }
        public string Chatlink { get; set; }
        public string Mode { get; set; }
        public bool Eqtab { get; set; }
        public int Profession { get; set; }
        public int Race { get; set; }
        public int[] Weapon { get; set; }
        public Skill Skill { get; set; }
        public int[][] Trait { get; set; }
        public Pet Pet { get; set; }
        public Equipment Equipment { get; set; }
    }

    internal class Skill
    {
        public Dictionary<string, int>[] T { get; set; }
    }

    internal class Pet
    {
        public int[] T { get; set; }
        public int[] A { get; set; }
    }

    internal class Equipment
    {
        public WeaponSet Weapon { get; set; }
        public Dictionary<string, EquipmentItem> Armor { get; set; }
        public Dictionary<string, EquipmentItem> Trinket { get; set; }
        public Buff Buff { get; set; }
        public int Relic { get; set; }
        public Fractal Fractal { get; set; }
        public object Jadebot { get; set; }
    }

    internal class WeaponSet
    {
        public WeaponEquipmentItem W11 { get; set; }
        public WeaponEquipmentItem W12 { get; set; }
        public WeaponEquipmentItem W21 { get; set; }
        public WeaponEquipmentItem W22 { get; set; }
        public EquipmentItem W31 { get; set; }
        public EquipmentItem W32 { get; set; }
    }

    internal class EquipmentItem
    {
        public int[] Item { get; set; }
        public int[][] Up { get; set; }
        public int[] Inf { get; set; }
    }

    internal class WeaponEquipmentItem : EquipmentItem
    {
        public string Type { get; set; }
    }

    internal class Buff
    {
        public int Food { get; set; }
        public int Utility { get; set; }
    }

    internal class Fractal
    {
        public int Mastery { get; set; }
        public int Potion { get; set; }
    }
}
