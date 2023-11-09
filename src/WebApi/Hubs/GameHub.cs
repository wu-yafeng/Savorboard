using GameSdk;
using GameSdk.Observers;
using GameSdk.Services;
using GameSdk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SignalR;
using Silos.Grains;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Channels;
using WebApi.Protocols;

namespace WebApi.Hubs
{
    [Authorize]
    public class GameHub : Hub<IMessageChannel>, IGameHub
    {
        private readonly IClusterClient _client;

        public GameHub(IClusterClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private async Task SubscribeAsync()
        {
            var world = _client.GetGrain<IGameWorld>(GetServerId());

            if (!await world.IsWorkingAsync())
            {
                await Clients.Caller.OnShowChatMsgAsync("MsgBox", "Server is not working.");

                return;
            }

            var channel = GetMessageChannel();

            var player = _client.GetGrain<IPlayer>(GetCurrentUserId());

            await player.SubscribeAsync(channel);
        }

        private async Task UnsubscribeAsync()
        {
            var channel = GetMessageChannel();

            var player = _client.GetGrain<IPlayer>(GetCurrentUserId());

            await player.UnsubscribeAsync(channel);
        }

        private IMessageChannel CreateChannel()
        {
            var channel = Context.Items["Caller_Channel"] as IMessageChannel;

            if (channel == null)
            {
                channel = Clients.Caller;
                Context.Items["Caller_Channel"] = channel;
            }

            return _client.CreateObjectReference<IMessageChannel>(channel);
        }

        private IMessageChannel GetMessageChannel()
        {
            var existingChannel = Context.Features.Get<IMessageChannel>();

            if (existingChannel == null)
            {
                existingChannel = CreateChannel();

                Context.Features.Set(existingChannel);
            }

            return existingChannel;
        }

        public override async Task OnConnectedAsync()
        {
            await SubscribeAsync();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await UnsubscribeAsync();

            await base.OnConnectedAsync();
        }

        public async Task HeartbeatAsync()
        {
            await SubscribeAsync();
        }

        public async Task<UBackpackViewModel> GetBackpackAsync()
        {
            var player = _client.GetGrain<IPlayer>(GetCurrentUserId());

            return await player.GetBackpackViewDataAsync();
        }

        private long GetCurrentUserId()
        {
            var uid = Context.User?.FindFirstValue("sub");

            return long.TryParse(uid, out var result) ? result : 1L;
        }

        private long GetServerId()
        {
            var uid = Context.User?.FindFirstValue("server_id");

            return long.TryParse(uid, out var result) ? result : 1L;
        }
    }
}
