using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.PersistenStates
{
    public class TickStatusData
    {
        public TickStatusData(DateTimeOffset currentTime)
        {
            CreatedTime = currentTime;
            FrameRefreshTime = currentTime;
            UpdateTime = currentTime;
            FrameRate = 0;
        }

        public DateTimeOffset CreatedTime { get; private set; }
        public DateTimeOffset UpdateTime { get; private set; }

        public DateTimeOffset FrameRefreshTime { get; private set; }
        public int FrameRate { get; private set; }

        public void Reset(DateTimeOffset currentTime)
        {
            FrameRefreshTime = currentTime;
            UpdateTime = currentTime;
        }

        public TimeSpan GetDeltaTime(DateTimeOffset currentTime)
        {
            var delta = currentTime - UpdateTime;

            UpdateTime = currentTime;

            FrameRate++;

            if (currentTime - FrameRefreshTime > TimeSpan.FromSeconds(1))
            {
                FrameRate = 0;
                FrameRefreshTime = currentTime;
            }

            return delta;
        }
    }
}
