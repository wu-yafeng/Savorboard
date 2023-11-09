using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    [GenerateSerializer]
    public class UAttributeData
    {
        [Id(0)]
        public double Attack { get; set; }

        [Id(1)]
        public double Armor { get; set; }

        [Id(2)]
        public double Health { get; set; }

        [Id(3)]
        public double MaxHealth { get; set; }
    }
}
