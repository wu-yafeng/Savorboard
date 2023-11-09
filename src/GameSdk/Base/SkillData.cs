using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Base
{
    [GenerateSerializer]
    public class SkillData
    {
        [Id(0)]
        public int Id { get; set; }
        [Id(1)]
        public int Atk { get; set; }
    }
}
