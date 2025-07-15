using GW2EquipmentBuildChecker.Core.GW2.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemDetails Details { get; set; }
    }

    public class ItemDetails
    {
        public string Type { get; set; }

        public EquipmentStats Infix_Upgrade { get; set; }
    }
}
