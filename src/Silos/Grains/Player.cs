using GameSdk;
using GameSdk.Base;
using GameSdk.Models;
using GameSdk.Observers;
using GameSdk.Parameters;
using GameSdk.Services;
using GameSdk.ViewModels;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Utilities;
using Silos.PersistenStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    [Reentrant]
    public partial class Player(
        [PersistentState("vip")] IPersistentState<VipData> vipData,
        [PersistentState("backpack")] IPersistentState<UBackpackData> backpack,
        [PersistentState("currentMap")] IPersistentState<UGameObjData> currentMap,
        IServiceClient<IMetaService> metaMgr,
        ILogger<Player> logger) : Grain, IPlayer
    {
        private readonly ObserverManager<IMessageChannel> _observerManager = new(TimeSpan.FromSeconds(10), logger);
        private readonly IPersistentState<VipData> _vipData = vipData;
        private readonly IPersistentState<UBackpackData> _backpack = backpack;
        private readonly IPersistentState<UGameObjData> _currentMap = currentMap;
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IServiceClient<IMetaService> _metaMgr = metaMgr ?? throw new ArgumentNullException(nameof(metaMgr));


        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (!_vipData.RecordExists)
            {
                _vipData.State = new VipData()
                {
                    ActivedTime = DateTimeOffset.Now,
                    AddExpTime = DateTimeOffset.Now,
                    CurExp = 0,
                    MaxExp = 10,
                    ExpiredTime = DateTimeOffset.Now.AddDays(30),
                    Level = 1,
                    AutoExtBackpack = true
                };

                await _vipData.WriteStateAsync();
            }

            await InitBackpack();

            if (!_currentMap.RecordExists)
            {
                var defaultMapId = await _metaMgr.Service.GetAllAsync<MapData>();
                var defaultMap = GrainFactory.GetGrain<IGameMap>(defaultMapId.First().Id);
                _currentMap.State = new UGameObjData()
                {
                    GameMap = defaultMap,
                    Position = new(1, 1)
                };
                await _currentMap.WriteStateAsync();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public Task<bool> IsDeadAsync()
        {
            // ply cannot die.
            return Task.FromResult(false);
        }

        public Task<int> GetObjTypeAsync()
        {
            return Task.FromResult(2);
        }

        public async Task OnHurtAsync(USkill skill, IGameObj attaker)
        {
            var objid = await attaker.GetObjTypeAsync();

            await _observerManager.Notify(channel => channel.OnHurtAsync(skill.Meta.Id, objid, "Monster"));
        }

        public async Task SetMapAsync(IGameMap map)
        {
            _currentMap.State.GameMap = map;

            await _currentMap.WriteStateAsync();
        }
    }
}
