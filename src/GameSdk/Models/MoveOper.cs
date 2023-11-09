using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Models
{
    [GenerateSerializer]
    public class MoveOper
    {
        [Id(0)]
        public Vecotr2D Vector { get; set; }

        [GenerateSerializer]
        public struct Vecotr2D(float X, float Y);
    }
}
