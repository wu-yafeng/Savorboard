using GameSdk;
using GameSdk.Base;
using GameSdk.Observers;
using GameSdk.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    public class GameWorld(IServiceClient<ITickService> tickMgr, IServiceClient<IMetaService> metaMgr) : Grain, IGameWorld
    {
        private readonly IServiceClient<ITickService> _tickMgr = tickMgr;
        private readonly List<IGameMap> _currentMaps = [];

        private readonly IServiceClient<IMetaService> _metaMgr = metaMgr;
        private long _currentMapId = 0;


        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var maps = await _metaMgr.Service.GetAllAsync<MapData>();

            foreach (var mapId in maps)
            {
                var map = GrainFactory.GetGrain<IGameMap>(mapId.Id);

                await map.SetDataAsync(mapId);

                _currentMaps.Add(map);
            }

            // GM can predefine 10_000 maps.
            _currentMapId = maps.Count() + 10000;

            await _tickMgr.Service.SubscribeAsync(this.AsReference<IGameWorld>());

            await base.OnActivateAsync(cancellationToken);
        }

        public Task<IEnumerable<IGameMap>> GetMapsAsync()
        {
            return Task.FromResult<IEnumerable<IGameMap>>(_currentMaps);
        }

        public async Task<bool> IsWorkingAsync()
        {
            return true;
        }
        public async Task TickAsync(TimeSpan deltaTime)
        {
            await Task.WhenAll(_currentMaps.Select(x => x.TickAsync(deltaTime)));
        }

        public async Task<IGameMap> CreateMapAsync(MapData mapData)
        {
            var copyMap = GrainFactory.GetGrain<IGameMap>(_currentMapId++);

            await copyMap.SetDataAsync(mapData);

            return copyMap;
        }
    }
}
