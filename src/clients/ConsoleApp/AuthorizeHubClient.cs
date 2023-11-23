using GameSdk;
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
    internal class AuthorizeHubClient : IAuthorizeHubClient, IAuthorizeHub
    {
        private readonly HubConnection _connection;

        public AuthorizeHubClient()
        {
            _connection = new HubConnectionBuilder().WithAutomaticReconnect()
                .WithUrl("http://localhost:5183/AuthorizeHub")
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

            foreach (var method in typeof(IAuthorizeHubClient).GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    _connection.On(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray(), (parameterValues) => InvokeHandler(this, method, parameterValues));
                }
            }

        }
        public Task StartAsync() =>
            _connection.StartAsync();
        public string? AccessToken { get; private set; }
        public bool? IsSucceed { get; set; }
        public Task OnSucceed(string access_token)
        {
            IsSucceed = true;
            AccessToken = access_token;

            Console.WriteLine("Login Success:{0}", access_token);

            return Task.CompletedTask;
        }

        public async Task SignInAsync(SignInReq context)
        {
            await _connection.SendAsync(nameof(SignInAsync), context);
        }

        public Task OnFailed(int ErrorCode, string Message, object? ExtensionData)
        {
            Console.WriteLine("Login failed[{0}]:{1}", ErrorCode, Message);
            IsSucceed = false;
            return Task.CompletedTask;
        }
    }
}
