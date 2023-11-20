using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Perf
{
    [EventSource(Name = "Silos.Perf.Tick")]
    public sealed class GameEventSource : EventSource
    {
        public static readonly GameEventSource Log = new();

        private readonly EventCounter _tickCounter;

        private GameEventSource()
        {
            _tickCounter = new("tick-count", this)
            {
                DisplayName = "Server Tick Count",
                DisplayUnits = "FPS"
            };
        }
        public void Tick(int fps)
        {
            WriteEvent(1, fps);

            _tickCounter.WriteMetric(fps);
        }
    }
}
