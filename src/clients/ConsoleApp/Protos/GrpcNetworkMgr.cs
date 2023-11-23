using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Protos
{
    public class GrpcNetworkMgr : IAsyncNetwork
    {
        private readonly GameHub.GameHubClient _client;
        private readonly ILogger<GrpcNetworkMgr> _logger;
        private readonly Queue<MessageEvent> _events = new();

        public GrpcNetworkMgr(GameHub.GameHubClient client, ILogger<GrpcNetworkMgr> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BackpackViewModel> GetBackpackDataViewModelAsync(GetBackpackViewModelPack messagePack)
        {
            return await _client.GetBackpackViewModelAsync(messagePack);
        }

        public Task<MessageEvent?> PeekAsync()
        {
            if(!_events.TryDequeue(out var result))
            {
                _logger.LogDebug("Empty queue.");
            }

            return Task.FromResult(result);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using var caller = _client.Subscribe(new SubscribeRequest(), cancellationToken: cancellationToken);
            while (!cancellationToken.IsCancellationRequested && await caller.ResponseStream.MoveNext())
            {
                _logger.LogDebug("Received message from server");
                _events.Enqueue(caller.ResponseStream.Current);
            }
        }
    }
}
