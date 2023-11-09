using GameSdk.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    [GenerateSerializer]
    public class USkill
    {
        [Id(0)]
        public int Id { get; set; }
        [Id(1)]
        public SkillData Meta { get; set; } = null!;
    }
}
