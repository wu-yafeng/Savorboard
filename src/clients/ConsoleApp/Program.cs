// See https://aka.ms/new-console-template for more information
using ConsoleApp;
using ConsoleApp.Protos;
using GameSdk;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;
using WebApi.Protocols;

bool gRpc = false;
bool signalR = true;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGrpcClient<GameHub.GameHubClient>(options =>
{
    options.Address = new Uri("http://localhost:5222/");
});

builder.Services.TryAddSingleton(new GameWorld());


if (gRpc)
{
    builder.Services.TryAddSingleton<IAsyncNetwork, GrpcNetworkMgr>();
}

if (signalR)
{
    builder.Services.TryAddSingleton<IAsyncNetwork, SignalRNetworkMgr>();
}

builder.Services.AddHostedService<NetworkMgr>();

builder.Services.AddHostedService<GameUIHost>();

var host = builder.Build();

await host.RunAsync();
