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

bool gRpc = false;
bool signalR = false;
while (!gRpc && !signalR)
{
    Console.WriteLine("Enter protos[gRpc/SignalR] you like:");

    var type = Console.ReadLine();

    switch(type)
    {
        case "gRpc":
            gRpc = true;
            break;
        case "signalR":
            signalR = true;
            break;
        default:
            Console.WriteLine("use default protos:gRpc");
            gRpc = true;
            break;
    }
}

var builder = Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime();

builder.ConfigureServices(services =>
{
    services.AddGrpcClient<GameHub.GameHubClient>(options =>
    {
        options.Address = new Uri("http://localhost:5222");
    });

    services.TryAddSingleton(new GameWorld());


    
    if(gRpc)
    {
        services.AddHostedService<GrpcNetworkMgr>();
    }

    if(signalR)
    {
        services.AddHostedService<SignalRNetworkMgr>();
    }
    services.AddHostedService<GameUIHost>();

});

var host = builder.Build();

await host.RunAsync();
