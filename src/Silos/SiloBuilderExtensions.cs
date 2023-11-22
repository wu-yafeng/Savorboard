using GameSdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            builder.UseOrleans(orleans =>
            {
                orleans.UseLocalhostClustering()
                    .AddMemoryGrainStorageAsDefault();
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
