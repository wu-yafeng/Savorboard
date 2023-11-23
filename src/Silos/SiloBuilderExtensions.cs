using GameSdk.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Silos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos
{
    public static class SiloBuilderExtensions
    {
        public static IHostBuilder UseSavorboard(this IHostBuilder builder)
        {
            builder.UseOrleans((context, orleans) =>
            {
                var connectionString = context.Configuration.GetConnectionString("MySql4Orleans");

                if (!string.IsNullOrEmpty(connectionString))
                {
                    orleans.UseAdoNetClustering(options =>
                    {
                        options.ConnectionString = connectionString;
                        options.Invariant = "MySql.Data.MySqlClient";
                    });
                }
                else
                {
                    orleans.UseLocalhostClustering();
                }

                orleans.AddMemoryGrainStorageAsDefault();

                var siloPort = context.Configuration.GetValue<int>("ORLEANS_SILOPORT", EndpointOptions.DEFAULT_SILO_PORT);
                var gatewayPort = context.Configuration.GetValue<int>("ORLEANS_GATEPORT", EndpointOptions.DEFAULT_GATEWAY_PORT);

                orleans.ConfigureEndpoints(siloPort, gatewayPort);
            });

            builder.ConfigureServices(services =>
            {
                services.AddGrainService<TickService>();
                services.AddGrainService<MetaService>();
                services.AddSingleton(typeof(IServiceClient<>), typeof(DefaultGrainServiceClient<>));
            });

            return builder;
        }
    }
}
