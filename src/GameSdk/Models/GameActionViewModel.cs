using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    [GenerateSerializer]
    public class GameActionViewModel
    {
        [Id(0)]
        public HashSet<PackItem> Packs { get; set; }
        [GenerateSerializer]
        public record PackItem(int Id, int PriceId, IEnumerable<RewardViewModel> Rewards);
    }
}
