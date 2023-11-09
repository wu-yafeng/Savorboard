using GameSdk;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class UGameObjData
    {
        public required IGameMap GameMap { get; set; }

        public required Point Position { get; set; }
    }
}
