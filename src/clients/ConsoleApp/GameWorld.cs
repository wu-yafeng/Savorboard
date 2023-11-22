using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public sealed class GameWorld
    {
        public Queue<MessageEvent> Messages { get; } = new();

        public string? KeyCharTop { get; set; }

        public LinkedList<string> ChatMsgs { get; set; } = new();
    }
}
