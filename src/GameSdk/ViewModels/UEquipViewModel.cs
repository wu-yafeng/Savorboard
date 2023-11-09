﻿using GameSdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.ViewModels
{
    [GenerateSerializer]
    public class UEquipViewModel
    {
        [Id(0)]
        public int ServerPos { get; set; }

        [Id(1)]
        public int BaseId { get; set; }

        [Id(2)]
        public UAttributeData Attribute { get; set; } = null!;

        [Id(3)]
        public WearSlot AllowedWearSlots { get; set; }
    }
}
