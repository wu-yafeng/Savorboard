using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    public enum WearSlot
    {
        Header = 1 << 0,
        Body = 1 << 1,
        Foot = 1 << 2,
        RHand = 1 << 3,
        LHand = 1 << 4
    }
}
