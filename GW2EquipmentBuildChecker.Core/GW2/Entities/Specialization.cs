using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2.Entities
{
    internal class Specialization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int[] Major_Traits { get; set; }
    }
}
