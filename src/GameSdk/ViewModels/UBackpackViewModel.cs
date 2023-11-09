using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.ViewModels
{
    [GenerateSerializer]
    public class UBackpackViewModel
    {
        [Id(0)]
        public UEquipViewModel[] Equips { get; set; } = null!;

        [Id(1)]
        public int MaxSize { get; set; }
    }
}
