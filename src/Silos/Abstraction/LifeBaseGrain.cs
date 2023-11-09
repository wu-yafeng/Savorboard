using GameSdk;
using GameSdk.Models;
using GameSdk.Services;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Silos.PersistenStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Abstraction
{
    public abstract class LifeBaseGrain<T>(IPersistentState<LifeBaseState> persistentState) : Grain, ILifeBase
        where T : LifeBaseState
    {
        protected IPersistentState<LifeBaseState> PersistentState { get; } = persistentState;

        public abstract Task<int> GetObjTypeAsync();

        public virtual Task<bool> IsDeadAsync()
        {
            return Task.FromResult(PersistentState.State.Attributes.Health <= 0);
        }

        public virtual Task UseSkillAsync(USkill skill, ILifeBase target)
        {

            return Task.CompletedTask;
        }
    }
}
