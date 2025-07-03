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
        public SkillSlot[] T { get; set; }
        public SkillSlot[] A { get; set; }
    }

    internal class SkillSlot
    {
        public Dictionary<string, int> Skills { get; set; } = new Dictionary<string, int>();
    }

    internal class Pet
    {
        public int[] T { get; set; }
        public int[] A { get; set; }
    }

    internal class Equipment
    {
        public WeaponSet Weapon { get; set; }
        public ArmorSet Armor { get; set; }
        public TrinketSet Trinket { get; set; }
        public Buff Buff { get; set; }
        public int Relic { get; set; }
        public Fractal Fractal { get; set; }
        public object Jadebot { get; set; }
    }

    internal class WeaponSet
    {
        public WeaponItem W11 { get; set; }
        public WeaponItem W12 { get; set; }
        public WeaponItem W21 { get; set; }
        public WeaponItem W22 { get; set; }
        public WeaponItem W31 { get; set; }
        public WeaponItem W32 { get; set; }
    }

    internal class WeaponItem
    {
        public int[] Item { get; set; }
        public int[][] Up { get; set; }
        public int[] Inf { get; set; }
    }

    internal class ArmorSet
    {
        public ArmorItem Boots { get; set; }
        public ArmorItem Leggings { get; set; }
        public ArmorItem Helm { get; set; }
        public ArmorItem Coat { get; set; }
        public ArmorItem Shoulders { get; set; }
        public ArmorItem Gloves { get; set; }
    }

    internal class ArmorItem
    {
        public int[] Item { get; set; }
        public int[][] Up { get; set; }
        public int[] Inf { get; set; }
    }

    internal class TrinketSet
    {
        public TrinketItem Earring1 { get; set; }
        public TrinketItem Earring2 { get; set; }
        public TrinketItem Ring1 { get; set; }
        public TrinketItem Ring2 { get; set; }
        public TrinketItem Back { get; set; }
        public TrinketItem Amulet { get; set; }
    }

    internal class TrinketItem
    {
        public int[] Item { get; set; }
        public int[][] Up { get; set; }
        public int[] Inf { get; set; }
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
