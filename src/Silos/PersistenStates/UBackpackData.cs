using GameSdk.Base;
using GameSdk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class UBackpackData
    {
        public int MaxSize { get; set; }
        public int Exp { get; set; }
        public int Level { get; set; }
        public required List<UEquip> Equips { get; set; }

        public required List<UItem> Items { get; set; }

        public ErrorCodes? RemoveItem(UItem item, int count)
        {
            var errorCode = default(ErrorCodes?);

            // always succeed.
            if (count < 0)
            {
                return null;
            }

            var expect = item.Count - count;

            if (expect < 0)
            {
                errorCode = ErrorCodes.BackPack_ItemCountOut;
            }
            else if (expect == 0)
            {
                Items.Remove(item);
            }
            else
            {
                item.Count = expect;
            }

            return errorCode;
        }

        public int GetCurrentSize()
        {
            return Equips.Count + Items.Count;
        }

        public UBackpackViewModel ToViewModel()
        {
            return new UBackpackViewModel()
            {
                Equips = Equips.Select(x => x.ToViewModel()).ToArray(),
                MaxSize = MaxSize
            };
        }

        public static UBackpackData Default()
        {
            return new UBackpackData()
            {
                Equips = [],
                Items = [],
                Level = default,
                MaxSize = default
            };
        }
    }

    public class UItem
    {
        public int ServerPos { get; set; }

        public int BaseId { get; set; }

        public int Count { get; set; }

        private ItemMeta? _baseMeta;

        public void SetMeta(ItemMeta meta)
        {
            _baseMeta = meta;
        }

        public ItemMeta GetMeta()
        {
            return _baseMeta ?? throw new BizException(ErrorCodes.RuntimeException);
        }
    }
}
