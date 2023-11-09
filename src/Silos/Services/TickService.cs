using GameSdk;
using GameSdk.Observers;
using GameSdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Utilities;
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
        private IDisposable? _timer;
        private readonly ILogger _logger = loggerFactory.CreateLogger<TickService>();
        private readonly HashSet<IGameTickable> _observerManager = new();

        private class TickState
        {
            public bool IsReset => UpdateTime.HasValue;

            public DateTimeOffset? UpdateTime { get; private set; }

            public DateTimeOffset FrameRefreshTime { get; private set; }
            public int FrameRate { get; private set; }

            public event Action<int> OnResetFrameRate = (fps) => { };

            public void Reset(DateTimeOffset currentTime)
            {
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

                var delta = currentTime - UpdateTime.Value;

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
        public override async Task Start()
        {
            _timer = RegisterTimer(UpdateAsync, new TickState(), TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(1));

            await base.Start();
        }

        public override async Task Init(IServiceProvider serviceProvider)
        {
            await base.Init(serviceProvider);
        }

        public override Task Stop()
        {
            _timer?.Dispose();
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
                device.OnResetFrameRate += (fps) => _logger.LogInformation("server fps -> {fps}", fps);
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
