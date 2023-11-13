using GameSdk.Models;
using GameSdk.Observers;
using GameSdk.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApi.Protocols;

namespace ConsoleApp
{
    public class GameHubClient : IGameHub, IMessageChannel
    {
        private readonly HubConnection _connection;

        public HubConnection Connection => _connection;
        public GameHubClient(string access_token)
        {
            _connection = new HubConnectionBuilder().WithAutomaticReconnect()
                .WithUrl("http://localhost:5183/GameHub", options => { options.AccessTokenProvider = () => Task.FromResult(access_token)!; })
                .Build();

            _connection.Reconnecting += async (error) =>
            {
                Console.WriteLine("[-] Connection losed, reconnection ...");
            };

            _connection.Reconnected += async (error) =>
            {
                Console.WriteLine("[-] Connection reconnected successfully ...");
            };

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
        }

        public async Task StartAsync() => await _connection.StartAsync();

        public Task HeartbeatAsync()
        {
            return _connection.SendAsync(nameof(HeartbeatAsync));
        }

        public Task OnEquipAddedAsync(UEquipViewModel equipAdded)
        {
            GameData.Shared.BackBuffer.AppendLine("You're received a new equip!");

            return Task.CompletedTask;
        }

        public Task<UBackpackViewModel> GetBackpackAsync()
        {
            return _connection.InvokeAsync<UBackpackViewModel>(nameof(GetBackpackAsync));
        }

        public Task OnShowChatMsgAsync(string name, string message)
        {
            GameData.Shared.BackBuffer.AppendLine($"Notify[{name}]:{message}");

            return Task.CompletedTask;
        }

        public Task OnGameObjExtiAsync(long id, string type)
        {
            GameData.Shared.BackBuffer.AppendLine($"Obj:[{id}]-{type} exit current map.");
            return Task.CompletedTask;
        }

        public Task OnHurtAsync(int skillid, int atker, string type)
        {
            GameData.Shared.BackBuffer.AppendLine($"you are attacked by {type}[{atker}] with skill[{skillid}]");
            return Task.CompletedTask;
        }
    }
}
