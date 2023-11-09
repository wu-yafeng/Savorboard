using GameSdk.Base;
using GameSdk.Services;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Silos.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Services
{
    [Reentrant]
    public class MetaService(GrainId grainId, Silo silo, ILoggerFactory loggerFactory) : GrainService(grainId, silo, loggerFactory), IMetaService
    {
        private readonly List<IMetaItem> _items = [];

        public async Task<T?> FindAsync<T>(int id) where T : IMetaItem
        {
            return _items.OfType<T>().FirstOrDefault(x => x.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : IMetaItem
        {
            return _items.OfType<T>().ToArray();
        }

        public override async Task Start()
        {
            // read meta from external system and watch events for update.

            _items.Add(new MapData(1, "DefaultEntryMap"));

            await base.Start();
        }
    }
}
