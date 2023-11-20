using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Parameters
{
    [GenerateSerializer]
    public class UpgradeBackpackPack
    {
        [Id(0)]
        public int TargetLevel { get; set; }
    }
}
