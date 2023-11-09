using GameSdk;
using GameSdk.Base;
using GameSdk.Observers;
using GameSdk.Services;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Services;
using Orleans.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    [Reentrant]
    public class GameMap(ILoggerFactory loggerFactory) : Grain, IGameMap
    {
        private readonly ObserverManager<IPlayer> _players = new(TimeSpan.FromMinutes(1), loggerFactory.CreateLogger<ObserverManager<IPlayer>>());
        private readonly ObserverManager<IGameMapObserver> _observerManager = new(TimeSpan.FromMinutes(1), loggerFactory.CreateLogger<ObserverManager<IGameMapObserver>>());
        private readonly List<IMonster> _monsters = new();
        private readonly ILogger _logger = loggerFactory.CreateLogger<GameMap>();
        private MapData? _metadata;

        public Task EnterAsync(IPlayer player)
        {
            _players.Subscribe(player, player);

            return Task.CompletedTask;
        }

        private TimeSpan _reportStateTime;
        public async Task TickAsync(TimeSpan deltaTime)
        {
            var tasks = new List<Task>();

            _players.ClearExpired();

            tasks.AddRange(_players.Select(x => x.TickAsync(deltaTime)));
            tasks.AddRange(_monsters.Select(x => x.TickAsync(deltaTime)));

            await Task.WhenAll(tasks);

            await SpawnMonsterAsync(deltaTime);

            _reportStateTime += deltaTime;

            if(_reportStateTime > TimeSpan.FromSeconds(5))
            {
                _reportStateTime = TimeSpan.Zero;

                _logger.LogInformation("Ply Count :{PlayerCount} Monster:{Count}", _players.Count, _monsters.Count);
            }
        }

        public Task ExitAsync(IPlayer player)
        {
            _players.Unsubscribe(player);

            return Task.CompletedTask;
        }

        private TimeSpan _monsterSpawnTime = TimeSpan.Zero;
        private int _monsterId = 0;
        private async Task SpawnMonsterAsync(TimeSpan deltaTime)
        {
            _monsterSpawnTime += deltaTime;

            if (_monsterSpawnTime > TimeSpan.FromSeconds(1))
            {
                _monsterSpawnTime = TimeSpan.Zero;

                if (Random.Shared.Next(0, 10000) < 8000)
                {
                    var monster = GrainFactory.GetGrain<IMonster>(_monsterId++);

                    await monster.SetAttributes(new()
                    {
                        Armor = 10,
                        Attack = 10,
                        Health = 100,
                        MaxHealth = 100
                    });

                    await monster.SetMapAsync(this.AsReference<IGameMap>());

                    _monsters.Add(monster);
                }
            }

            var copyOnDelete = _monsters.ToArray();

            for (var i = 0; i < copyOnDelete.Length; i++)
            {
                if (await copyOnDelete[i].IsDeadAsync())
                {
                    _monsters.RemoveAt(i);
                    await _observerManager.Notify(observer => observer.OnGameObjExtiAsync(copyOnDelete[i].GetPrimaryKeyLong(), "Monster"));
                }
            }
        }

        public Task SubscribeAsync(IGameMapObserver observer)
        {
            _observerManager.Subscribe(observer, observer);

            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(IGameMapObserver observer)
        {
            _observerManager.Unsubscribe(observer);

            return Task.CompletedTask;
        }

        public async Task SetDataAsync(MapData data)
        {
            _metadata = data;
        }

        public async Task<IEnumerable<IGameObj>> SearchAsync(int width, int height)
        {
            _players.ClearExpired();

            return _players.Select(x=>x.AsReference<IGameObj>()).ToArray();
        }
    }
}
