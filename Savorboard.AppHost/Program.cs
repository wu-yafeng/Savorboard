var builder = DistributedApplication.CreateBuilder(args);

var grpc = builder.AddProject<Projects.GrpcService>("grpcservice");

var signalr = builder.AddProject<Projects.WebApi>("webapi");

builder.AddProject<Projects.ConsoleApp>("consoleapp")
    .WithReference(grpc)
    .WithReference(signalr);

builder.Build().Run();
