// See https://aka.ms/new-console-template for more information
using ConsoleApp;
using GameSdk;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;
using System.Text;
using WebApi.Protocols;

Console.WriteLine("Welcome to Savorboard!");

var authorize = new AuthorizeHubClient();

await authorize.StartAsync();

var id = 0L;
var pwd = string.Empty;
var sid = 0;
while (string.IsNullOrEmpty(authorize.AccessToken))
{
    if (id > 0 && !string.IsNullOrEmpty(pwd))
    {
        await Task.Delay(TimeSpan.FromSeconds(1));

        continue;
    }

    Console.Write("ID:");
    long.TryParse(Console.ReadLine(), out id);
    Console.Write("PWD:");
    pwd = Console.ReadLine();

    Console.Write("ServerId:");
    int.TryParse(Console.ReadLine(), out sid);

    Console.WriteLine("Login Server [1] with credentials-> id:{0} pwd:{1}", id, pwd);
    await authorize.SignInAsync(new SignInReq(id, pwd, sid));
}

var hub = new GameHubClient(authorize.AccessToken);

GameData.Shared = new()
{
    Network = hub,
    HearbeatClock = TimeSpan.Zero,
    UpdateTime = DateTimeOffset.Now
};

await hub.StartAsync();

Console.WriteLine("GameHub connection successfully.");

ConsoleKeyInfo? peekMessage()
{
    if (Console.KeyAvailable)
    {
        return Console.ReadKey();
    }

    return null;
}

Console.WriteLine("Press 'Enter' to start game loop.");
// wait input enter to enter the game loop.
while (Console.ReadKey().Key != ConsoleKey.Enter)
{
    Console.WriteLine("invalid input, please press key 'Enter'");
}



async Task HandleInput(ConsoleKeyInfo message)
{
    var sb = GameData.Shared.BackBuffer;

    sb.AppendLine($"Key-> {message.KeyChar}");

    // prev map
    if (message.Key == ConsoleKey.UpArrow)
    {
        sb.AppendLine("enter world");
    }

    if (message.Key == ConsoleKey.F2)
    {
        sb.AppendLine("query backpack");

        var backpack = await hub.GetBackpackAsync();

        sb.Append($"result is -> {backpack.MaxSize} eqpcount->{backpack.Equips.Length}");
    }
}

while (true)
{
    var now = DateTimeOffset.Now;

    var deltaTime = now - GameData.Shared.UpdateTime;

    GameData.Shared.UpdateTime = now;

    var message = peekMessage();

    // handle input
    if (message != null)
    {
        await HandleInput(message.Value);
    }

    GameData.Shared.HearbeatClock = GameData.Shared.HearbeatClock.Add(deltaTime);

    if (GameData.Shared.HearbeatClock > TimeSpan.FromSeconds(10))
    {
        GameData.Shared.HearbeatClock = TimeSpan.Zero;

        await GameData.Shared.Network.HeartbeatAsync();
    }

    // represent backbuffer.
    Console.Clear();

    if(GameData.Shared.BackBuffer.ToString().Split('\n').Count() > 50)
    {
        GameData.Shared.BackBuffer.Clear();
    }

    Console.Write(GameData.Shared.BackBuffer);

    // 100fps
    await Task.Delay(10);
}
