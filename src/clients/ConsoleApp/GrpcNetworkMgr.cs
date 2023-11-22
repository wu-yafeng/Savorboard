using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class GrpcNetworkMgr : BackgroundService
    {
        private readonly GameHub.GameHubClient _client;
        private readonly ILogger<GrpcNetworkMgr> _logger;
        private readonly GameWorld _world;

        public GrpcNetworkMgr(GameHub.GameHubClient client, ILogger<GrpcNetworkMgr> logger, GameWorld world)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var caller = _client.Subscribe(new SubscribeRequest(), cancellationToken: stoppingToken);
            while (!stoppingToken.IsCancellationRequested && await caller.ResponseStream.MoveNext())
            {
                _world.Messages.Enqueue(caller.ResponseStream.Current);
            }
        }
    }
}
