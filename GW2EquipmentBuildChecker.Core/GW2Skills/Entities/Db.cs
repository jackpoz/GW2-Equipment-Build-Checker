using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2Skills.Entities
{
    internal class Db
    {
        public DbRows Profession { get; set; }
        public DbRows PrflType { get; set; }
        public DbRows Profile { get; set; }
        public DbRows Rarity { get; set; }
        public DbRows Specialization { get; set; }
        public DbRows Upgrade { get; set; }
        public DbRows Uptype { get; set; }
        public DbRows Weapon { get; set; }
    }

    internal class DbRows
    {
        public JsonElement[][] Rows { get; set; }
    }
}
