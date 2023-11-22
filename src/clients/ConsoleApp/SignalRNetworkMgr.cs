﻿using GameSdk.Models;
using GameSdk.Observers;
using GameSdk.ViewModels;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebApi.Protocols;

namespace ConsoleApp
{
    public class SignalRNetworkMgr(GameWorld world) : BackgroundService, IMessageChannel
    {
        private readonly GameWorld _world = world;

        public Task OnEquipAddedAsync(UEquipViewModel equipAdded)
        {
            _world.Messages.Enqueue(new MessageEvent()
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
            _world.Messages.Enqueue(new MessageEvent()
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
            _world.Messages.Enqueue(new MessageEvent()
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
            _world.Messages.Enqueue(new MessageEvent()
            {
                Data = Any.Pack(new ChatMsg()
                {
                    Channel = 2,
                    Content = $"you are attacked by {type}[{atker}] with skill[{skillid}]"
                }),
            });
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var authorize = new AuthorizeHubClient();

            await authorize.StartAsync();

            await authorize.SignInAsync(new SignInReq(1, "1", 1));

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

            var _connection = new HubConnectionBuilder().WithAutomaticReconnect()
                 .WithUrl("http://localhost:5183/GameHub", options => { options.AccessTokenProvider = () => Task.FromResult(authorize.AccessToken)!; })
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

            await _connection.StartAsync(stoppingToken);
        }
    }
}
