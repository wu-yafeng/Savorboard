using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Protocols;

namespace ConsoleApp
{
    public class GameData
    {
        public static GameData? Shared { get; set; }

        public StringBuilder BackBuffer { get; set; } = new StringBuilder();

        public required IGameHub Network { get; set; }

        public required TimeSpan HearbeatClock { get; set; }

        public required DateTimeOffset UpdateTime { get; set; }
    }
}
