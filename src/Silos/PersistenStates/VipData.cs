using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class VipData
    {
        public DateTimeOffset ActivedTime { get; set; }

        public DateTimeOffset ExpiredTime { get; set; }

        public double CurExp { get; set; }

        public double MaxExp { get; set; }
        public DateTimeOffset AddExpTime { get; set; }

        public int Level { get; set; }
    }
}
