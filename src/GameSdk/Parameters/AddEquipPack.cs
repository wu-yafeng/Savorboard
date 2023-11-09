using GameSdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Parameters
{
    [GenerateSerializer]
    public class AddEquipPack
    {
        [Id(0)]
        public int BaseId { get; set; }

        [Id(1)]
        public UAttributeData Attributes { get; set; } = null!;

        [Id(2)]
        public WearSlot AllowedWearSlots { get; set; }
    }
}
