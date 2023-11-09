using GameSdk.Models;
using GameSdk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class UEquip
    {
        public WearSlot AllowedWearSlots { get; set; }

        public int BaseId { get; set; }

        public required UAttributeData Attributes { get; set; }

        public UEquipViewModel ToViewModel(int serverPos)
        {
            return new()
            {
                ServerPos = serverPos,
                Attribute = Attributes,
                BaseId = BaseId,
                AllowedWearSlots = AllowedWearSlots
            };
        }
    }
}
