using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public sealed class GameWorld
    {
        public string? KeyCharTop { get; set; }

        public LinkedList<string> ChatMsgs { get; set; } = new();

        public StringBuilder Surface { get; } = new();

        public StringBuilder BackpackView { get; } = new();
    }
}
