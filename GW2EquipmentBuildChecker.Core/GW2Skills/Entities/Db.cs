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
        public Specialization Specialization { get; set; }
    }

    internal class Specialization
    {
        public JsonElement[][] Rows { get; set; }
    }
}
