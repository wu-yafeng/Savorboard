using ConsoleApp.Protos;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    /// <summary>
    /// Game Network Thread. Sync state from server to local.
    /// </summary>
    /// <param name="network"></param>
    public class NetworkMgr(IAsyncNetwork network) : BackgroundService
    {
        private IAsyncNetwork _network = network ?? throw new ArgumentNullException(nameof(network));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _network.StartAsync(stoppingToken);
        }
    }
}
