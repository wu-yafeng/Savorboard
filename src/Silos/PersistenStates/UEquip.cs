using GameSdk.Models;
using GameSdk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class UEquip : IEquatable<UEquip?>
    {
        public int ServerPos { get; set; }
        public WearSlot AllowedWearSlots { get; set; }

        public int BaseId { get; set; }

        public required UAttributeData Attributes { get; set; }

        public UEquipViewModel ToViewModel()
        {
            return new()
            {
                ServerPos = ServerPos,
                Attribute = Attributes,
                BaseId = BaseId,
                AllowedWearSlots = AllowedWearSlots
            };
        }


        public override bool Equals(object? obj)
        {
            return Equals(obj as UEquip);
        }

        public bool Equals(UEquip? other)
        {
            return other is not null &&
                   ServerPos == other.ServerPos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ServerPos);
        }


        public static bool operator ==(UEquip? left, UEquip? right)
        {
            return EqualityComparer<UEquip>.Default.Equals(left, right);
        }

        public static bool operator !=(UEquip? left, UEquip? right)
        {
            return !(left == right);
        }
    }
}
