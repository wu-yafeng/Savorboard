using GameSdk.Models;
using GameSdk.Observers;
using GameSdk.Parameters;
using GameSdk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk
{
    public interface IPlayer : IGameTickable, IGrainWithIntegerKey, ILifeBase
    {
        Task SubscribeAsync(IMessageChannel channel);
        Task UnsubscribeAsync(IMessageChannel channel);
        Task<UBackpackViewModel> GetBackpackViewDataAsync();
        Task AddEquipAsync(AddEquipPack message);

        Task SetMapAsync(IGameMap map);

        Task OnHurtAsync(USkill skill, IGameObj attaker);
        Task UpgradeBackpackAsync(UpgradeBackpackPack messagePack);
    }
}
