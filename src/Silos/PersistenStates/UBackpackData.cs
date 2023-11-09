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

        public required IList<UEquip> Equips { get; set; }

        public static UBackpackData Default()
        {
            return new UBackpackData()
            {
                Equips = new List<UEquip>(),
                MaxSize = 50
            };
        }

        public UBackpackViewModel ToViewModel()
        {
            return new UBackpackViewModel()
            {
                Equips = Equips.Select((x, i) => x.ToViewModel(i)).ToArray(),
                MaxSize = MaxSize
            };
        }
    }
}
