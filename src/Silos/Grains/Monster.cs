using GameSdk;
using GameSdk.Models;
using GameSdk.Observers;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    [Reentrant]
    public class Monster(ILogger<Monster> logger,
        [PersistentState("attributes")] IPersistentState<UAttributeData> attributes) : Grain, IMonster
    {
        private TimeSpan _showChatMsg = TimeSpan.Zero;
        private IGameMap _currentMap;
        private readonly ObserverManager<IMonsterObserver> _observerManager = new(TimeSpan.FromMinutes(5), logger);

        private readonly IPersistentState<UAttributeData> _attributes = attributes;

        public Task<bool> IsDeadAsync()
        {
            return Task.FromResult(_attributes.State.Health <= 0);
        }

        public async Task TickAsync(TimeSpan deltaTime)
        {
            _showChatMsg += deltaTime;

            if (_showChatMsg > TimeSpan.FromSeconds(10))
            {
                await Task.WhenAll(_observerManager.Select(x => x.OnShowChatMsgAsync($"Monster[{IdentityString}]", "I'm the monster!")));
            }

            if (_showChatMsg > TimeSpan.FromSeconds(5) && _currentMap != null)
            {
                // TEST SKILL
                var skill = new USkill()
                {
                    Id = 1,
                    Meta = new()
                    {
                        Id = 1,
                        Atk = 10
                    }
                };

                // boss skill is mutiple targets.
                var targets = await _currentMap.SearchAsync(999, 999);

                await Task.WhenAll(targets.Select(x => UseSkillAsync(skill, x)));
            }

            _showChatMsg = TimeSpan.Zero;
        }

        public async Task UseSkillAsync(USkill skill, IGameObj target)
        {
            if (target is IPlayer player)
            {
                await player.OnHurtAsync(skill, this.AsReference<IMonster>());
            }
        }

        public Task<int> GetObjTypeAsync()
        {
            return Task.FromResult(1);
        }

        public async Task SetAttributes(UAttributeData attributes)
        {
            _attributes.State = attributes;
            await _attributes.WriteStateAsync();
        }

        public Task SetMapAsync(IGameMap map)
        {
            _currentMap = map;

            return Task.CompletedTask;
        }
    }
}
