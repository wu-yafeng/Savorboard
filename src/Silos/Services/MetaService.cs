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

            // 20 levels for backpack,each level exp is same.
            _items.Add(new BackpackData(0, 50, 0));
            for (var i = 1; i <= 20; i++)
            {
                _items.Add(new BackpackData(i, i * 5, i * 2));
            }

            // items
            for (var i = 1; i < 5000; i++)
            {
                _items.Add(new ItemMeta(i, ItemMeta.FeatureType.AddBackpackExp, i * Random.Shared.Next(1, 10), 0));
            }

            await base.Start();
        }
    }

}
