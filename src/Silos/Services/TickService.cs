using GameSdk;
using GameSdk.Observers;
using GameSdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Utilities;
using Silos.Perf;
using Silos.PersistenStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Services
{
    [Reentrant]
    public class TickService(GrainId grainId, Silo silo, ILoggerFactory loggerFactory) : GrainService(grainId, silo, loggerFactory), ITickService
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TickService>();
        private readonly HashSet<IGameTickable> _observerManager = new();

        private class TickState
        {
            public bool IsReset { get; private set; }

            public DateTimeOffset UpdateTime { get; private set; }

            public DateTimeOffset FrameRefreshTime { get; private set; }
            public int FrameRate { get; private set; }

            public event Action<int> OnResetFrameRate = (fps) => { };

            public void Reset(DateTimeOffset currentTime)
            {
                IsReset = true;
                FrameRefreshTime = currentTime;
                UpdateTime = currentTime;
                FrameRate = 0;
            }

            public TimeSpan GetDeltaTime(DateTimeOffset currentTime)
            {
                if (!IsReset)
                {
                    throw new InvalidOperationException("Need reset tick state.");
                }

                var delta = currentTime - UpdateTime;

                UpdateTime = currentTime;

                FrameRate++;

                if (currentTime - FrameRefreshTime > TimeSpan.FromSeconds(1))
                {
                    OnResetFrameRate(FrameRate);
                    FrameRate = 0;
                    FrameRefreshTime = currentTime;
                }

                return delta;
            }
        }

        protected override async Task StartInBackground()
        {
            await base.StartInBackground();

            var state = new TickState();

            while (!StoppedCancellationTokenSource.IsCancellationRequested)
            {
                var prevframe = state.UpdateTime.Add(TimeSpan.FromMilliseconds(1000F / 128F));
                await UpdateAsync(state);

                // lock 128fps, timePerFrame = 1000 / 128;

                var next = prevframe - DateTimeOffset.Now;

                if (next > TimeSpan.Zero)
                {
                    await Task.Delay(next);
                }
            }
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            await base.Init(serviceProvider);
        }

        public override Task Stop()
        {
            return base.Stop();
        }

        private async Task UpdateAsync(object state)
        {
            if (state is not TickState device)
            {
                return;
            }

            if (!device.IsReset)
            {
                device.Reset(DateTimeOffset.Now);
                device.OnResetFrameRate += (fps) => GameEventSource.Log.Tick(fps);
            }

            var deltaTime = device.GetDeltaTime(DateTimeOffset.Now);
            await Task.WhenAll(_observerManager.Select(t => t.TickAsync(deltaTime)));
        }

        public Task SubscribeAsync(IGameTickable observer)
        {
            _observerManager.Add(observer);

            return Task.CompletedTask;
        }
    }
}
