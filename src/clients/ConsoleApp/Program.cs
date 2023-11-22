// See https://aka.ms/new-console-template for more information
using ConsoleApp;
using GameSdk;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;
using WebApi.Protocols;

bool gRpc = true;
bool signalR = false;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpcClient<GameHub.GameHubClient>(options =>
{
    options.Address = new Uri("http://grpcservice");
});

builder.Services.TryAddSingleton(new GameWorld());


if (gRpc)
{
    builder.Services.AddHostedService<GrpcNetworkMgr>();
}

if (signalR)
{
    builder.Services.AddHostedService<SignalRNetworkMgr>();
}
builder.Services.AddHostedService<GameUIHost>();

var host = builder.Build();

await host.RunAsync();
