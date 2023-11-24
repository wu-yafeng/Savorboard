using GameSdk.Observers;
using GameSdk.ViewModels;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GrpcService;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WebApi.Protocols;

namespace ConsoleApp.Protos
{
    public class ByteStringConverter : JsonConverter<ByteString>
    {
        public override ByteString? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return ByteString.CopyFromUtf8(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ByteString value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToStringUtf8());
        }
    }

    public class SignalRNetworkMgr : IAsyncNetwork, IGameHub
    {
        private readonly Queue<Any> _events = new();
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
                 .AddJsonProtocol(options =>
                 {
                     options.PayloadSerializerOptions.Converters.Add(new ByteStringConverter());
                 })
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

        public Task<Any?> PeekAsync()
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

            var channel = await Subscribe(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                _events.Enqueue(await channel.ReadAsync(cancellationToken));
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

        public async Task<ChannelReader<Any>> Subscribe(CancellationToken cancellationToken = default)
        {
            return await _connection.StreamAsChannelAsync<Any>(nameof(Subscribe));
        }
    }
}
