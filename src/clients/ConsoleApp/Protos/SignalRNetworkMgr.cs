using GameSdk.Observers;
using GameSdk.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Protocols;

namespace ConsoleApp.Protos
{
    public class SignalRNetworkMgr : IAsyncNetwork, IMessageChannel, IGameHub
    {
        private readonly Queue<MessageEvent> _events = new();
        private readonly HubConnection _connection;
        private readonly ILogger _logger;

        private static async Task<string?> GetAccessTokenAsync()
        {
            var authorize = new AuthorizeHubClient();
            await authorize.StartAsync();

            await authorize.SignInAsync(new SignInReq(1, "1", 1));

            while (string.IsNullOrEmpty(authorize.AccessToken) && authorize.IsSucceed == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            authorize.IsSucceed = null;

            return authorize.AccessToken;
        }

        public SignalRNetworkMgr(ILogger<SignalRNetworkMgr> logger)
        {
            _connection = new HubConnectionBuilder().WithAutomaticReconnect()
                 .WithUrl("http://localhost:5183/GameHub", options => { options.AccessTokenProvider = GetAccessTokenAsync; })
                 .Build();

            // register all handlers.
            static async Task InvokeHandler(object caller, MethodInfo method, params object?[] parameters)
            {
                var result = method.Invoke(caller, parameters);

                await (Task)result!;
            }

            foreach (var method in typeof(IMessageChannel).GetInterfaces().SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance)))
            {
                if (method.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    _connection.On(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray(), (parameterValues) => InvokeHandler(this, method, parameterValues));
                }
            }

            _logger = logger;
        }

        public Task OnEquipAddedAsync(UEquipViewModel equipAdded)
        {
            _events.Enqueue(new MessageEvent()
            {
                Data = Any.Pack(new ChatMsg()
                {
                    Channel = 2,
                    Content = $"Server Pack {JsonSerializer.Serialize(equipAdded)}"
                }),
            });
            return Task.CompletedTask;
        }

        public Task OnShowChatMsgAsync(string name, string message)
        {
            _events.Enqueue(new MessageEvent()
            {
                Data = Any.Pack(new ChatMsg()
                {
                    Channel = 2,
                    Content = $"{name}:::{message}"
                }),
            });

            return Task.CompletedTask;
        }

        public Task OnGameObjExtiAsync(long id, string type)
        {
            _events.Enqueue(new MessageEvent()
            {
                Data = Any.Pack(new ChatMsg()
                {
                    Channel = 2,
                    Content = $"Obj:[{id}]-{type} exit current map."
                }),
            });
            return Task.CompletedTask;
        }

        public Task OnHurtAsync(int skillid, int atker, string type)
        {
            _events.Enqueue(new MessageEvent()
            {
                Data = Any.Pack(new ChatMsg()
                {
                    Channel = 2,
                    Content = $"you are attacked by {type}[{atker}] with skill[{skillid}]"
                }),
            });
            return Task.CompletedTask;
        }

        public Task<MessageEvent?> PeekAsync()
        {
            if (!_events.TryDequeue(out var result))
            {
                _logger.LogDebug("Empty queue.");
            }

            return Task.FromResult(result);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await _connection.StartAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await HeartbeatAsync();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        public async Task<BackpackViewModel> GetBackpackDataViewModelAsync(GetBackpackViewModelPack messagePack)
        {
            var signalR = await GetBackpackAsync();

            return new BackpackViewModel
            {
                MaxSize = signalR.MaxSize
            };
        }

        public Task<UBackpackViewModel> GetBackpackAsync()
        {
            return _connection.InvokeAsync<UBackpackViewModel>(nameof(GetBackpackAsync));
        }

        public Task HeartbeatAsync()
        {
            return _connection.SendAsync(nameof(HeartbeatAsync));
        }
    }
}
