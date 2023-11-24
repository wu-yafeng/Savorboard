using GameSdk;
using GameSdk.Observers;
using GameSdk.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Savorboard.Protocols;
using System.Text.Json;

namespace GrpcService.Services
{
    public class GameHubService : GameHub.GameHubBase
    {
        private readonly IClusterClient _client;

        public GameHubService(IClusterClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private class GrpcMessageChannel : IMessageChannel
        {
            private readonly IServerStreamWriter<Any> _responseStream;

            public GrpcMessageChannel(IServerStreamWriter<Any> responseStream)
            {
                _responseStream = responseStream ?? throw new ArgumentNullException(nameof(responseStream));
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
                await _responseStream.WriteAsync(Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = $"{name}:::{message}"
                }));
            }
        }

        private long GetUserId(ServerCallContext context)
        {
            var userid = context.AuthContext.IsPeerAuthenticated ? 1 : 1;

            return userid;
        }

        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<Any> responseStream, ServerCallContext context)
        {
            // retrive from auth context.
            var userid = GetUserId(context);

            var world = _client.GetGrain<IGameWorld>(1);

            if (!await world.IsWorkingAsync())
            {
                await responseStream.WriteAsync(Any.Pack(new ChatMsg()
                {
                    Channel = 1,
                    Content = "server is not working."
                }));
            }

            var currentPlayer = _client.GetGrain<IPlayer>(userid);

            var channel = new GrpcMessageChannel(responseStream);

            var reference = _client.CreateObjectReference<IMessageChannel>(channel);

            // subscribe for aliving.
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await currentPlayer.SubscribeAsync(reference);

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        public override async Task<BackpackViewModel> GetBackpackViewModel(GetBackpackViewModelPack request, ServerCallContext context)
        {
            var userid = GetUserId(context);

            var currentPlayer = _client.GetGrain<IPlayer>(userid);

            var viewmodel = await currentPlayer.GetBackpackViewDataAsync();

            return new BackpackViewModel()
            {
                MaxSize = viewmodel.MaxSize
            };
        }
    }
}
