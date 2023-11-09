using GameSdk.Base;
using GameSdk.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk
{
    public interface IGameMap : IGrainWithIntegerKey, IGameTickable
    {
        Task EnterAsync(IPlayer player);
        Task ExitAsync(IPlayer player);

        Task SubscribeAsync(IGameMapObserver observer);
        Task UnsubscribeAsync(IGameMapObserver observer);

        Task<IEnumerable<IGameObj>> SearchAsync(int width, int height);

        Task SetDataAsync(MapData data);
    }
}
