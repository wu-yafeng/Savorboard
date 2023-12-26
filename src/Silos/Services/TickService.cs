using GameSdk;
using GameSdk.Observers;
using GameSdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Utilities;
using Silos.Perf;
using Silos.PersistenStates;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Services
{
    [Reentrant]
    public class TickService(GrainId grainId, Silo silo, ILoggerFactory loggerFactory, IHostEnvironment environment, DiagnosticHelper diagnostic)
        : GrainService(grainId, silo, loggerFactory), ITickService
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<TickService>();
        private readonly HashSet<IGameTickable> _observerManager = [];
        private readonly IHostEnvironment _environment = environment;
        private readonly DiagnosticHelper _diagnostic = diagnostic;

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

            var duration = TimeSpan.FromSeconds(1);

            while (!StoppedCancellationTokenSource.IsCancellationRequested)
            {
                var prevframe = state.UpdateTime.Add(duration);
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
            using var activity = _diagnostic.Trace.StartActivity("TickService.UpdateAsync");

            if (!device.IsReset)
            {
                activity?.SetTag("activity.reset", "true");

                var histogram = _diagnostic.Metric.CreateHistogram<int>("server_tick", "fps", "server tick frame per seconds.");

                device.Reset(DateTimeOffset.Now);
                device.OnResetFrameRate += (fps) => histogram.Record(fps);
            }

            var deltaTime = device.GetDeltaTime(DateTimeOffset.Now);
            activity?.SetTag("activity.delta_ms", deltaTime.TotalMilliseconds);
            await Task.WhenAll(_observerManager.Select(t => t.TickAsync(deltaTime)));
        }

        public Task SubscribeAsync(IGameTickable observer)
        {
            _observerManager.Add(observer);

            return Task.CompletedTask;
        }
    }
}
