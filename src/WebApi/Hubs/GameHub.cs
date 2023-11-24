using GameSdk;
using GameSdk.Observers;
using GameSdk.Services;
using GameSdk.ViewModels;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SignalR;
using Savorboard.Protocols;
using Silos.Grains;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        public async Task<ChannelReader<Any>> Subscribe(CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateUnbounded<Any>();

            var world = _client.GetGrain<IGameWorld>(1);

            if (!await world.IsWorkingAsync())
            {
                channel.Writer.Complete();
                return channel.Reader;
            }

            _ = WriteChannelAsync(channel.Writer, cancellationToken);

            return channel.Reader;
        }

        private async Task WriteChannelAsync(ChannelWriter<Any> writer, CancellationToken cancellationToken = default)
        {
            var player = _client.GetGrain<IPlayer>(GetCurrentUserId());
            var channel = new SignalRMessageChannel(writer, cancellationToken);

            var reference = _client.CreateObjectReference<IMessageChannel>(channel);


            while (!cancellationToken.IsCancellationRequested)
            {
                await player.SubscribeAsync(reference);

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }

            await player.UnsubscribeAsync(reference);
        }


        private class SignalRMessageChannel : IMessageChannel
        {
            private readonly ChannelWriter<Any> _responseStream;
            private readonly CancellationToken _cancellationToken;

            public SignalRMessageChannel(ChannelWriter<Any> channelWriter, CancellationToken cancellationToken)
            {
                _responseStream = channelWriter ?? throw new ArgumentNullException(nameof(channelWriter));
                _cancellationToken = cancellationToken;
            }

            public async Task OnEquipAddedAsync(UEquipViewModel equipAdded)
            {
                await _responseStream.WriteAsync(Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = JsonSerializer.Serialize(equipAdded)
                }));
            }

            public async Task OnGameObjExtiAsync(long id, string type)
            {
                await _responseStream.WriteAsync(Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = $"id:{id},type:{type}"
                }));
            }

            public async Task OnHurtAsync(int skillid, int atker, string type)
            {
                await _responseStream.WriteAsync(Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = $"skill:{skillid} atker:{atker} type:{type}"
                }));
            }
            
            public async Task OnShowChatMsgAsync(string name, string message)
            {
                var pack = Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = $"{name}:::{message}"
                });

                await _responseStream.WriteAsync(pack, _cancellationToken);
            }
        }
    }
}
