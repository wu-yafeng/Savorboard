using ConsoleApp.Protos;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class NetworkMgr(IAsyncNetwork network) : BackgroundService
    {
        private IAsyncNetwork _network = network ?? throw new ArgumentNullException(nameof(network));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _network.StartAsync(stoppingToken);
        }
    }
}
