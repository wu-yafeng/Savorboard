using GameSdk.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Base
{
    [GenerateSerializer]
    public record BackpackData(int Id, int ExtSize, int Exp) : IMetaItem;
}
