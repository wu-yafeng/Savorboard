using GameSdk.Models;
using GameSdk.ViewModels;

namespace GameSdk.Observers
{
    public interface IPlayerObserver : IChat, IGrainObserver
    {

        Task OnEquipAddedAsync(UEquipViewModel equipAdded);

        Task OnHurtAsync(int skillid, int atker, string type);
    }
}
