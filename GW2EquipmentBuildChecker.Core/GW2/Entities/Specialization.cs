using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2EquipmentBuildChecker.Core.GW2.Entities
{
    public class Specialization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Major_Traits { get; set; }
        public int GetMajorTraitIndex(int? traitId)
        {
            if (traitId == null)
                return -1;
            return Major_Traits.IndexOf(traitId.Value) % 3;
        }
    }
}
