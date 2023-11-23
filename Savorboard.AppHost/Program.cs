using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("GameCluster");

builder.AddProject<Projects.GrpcService>("grpcservice")
    .AsHttp2Service()
    .WithEnvironment("CONNECTIONSTRINGS__MYSQL4ORLEANS", connectionString);

builder.AddProject<Projects.WebApi>("webapi")
    .WithEnvironment("CONNECTIONSTRINGS__MYSQL4ORLEANS", connectionString);

builder.Build().Run();
