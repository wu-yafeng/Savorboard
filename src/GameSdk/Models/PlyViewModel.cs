using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    [GenerateSerializer]
    public class PlyViewModel
    {
        [Id(0)]
        public long Id { get; set; }

        [Id(1)]
        public string Name { get; set; } = null!;
    }
}
