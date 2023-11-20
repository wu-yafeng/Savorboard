using GameSdk.Observers;
using GameSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Grains
{
    public partial class Player
    {
        public Task SubscribeAsync(IMessageChannel channel)
        {
            _observerManager.Subscribe(channel, channel);

            _currentMap.State.GameMap.EnterAsync(this.AsReference<IPlayer>());
            _currentMap.State.GameMap.SubscribeAsync(channel);

            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(IMessageChannel channel)
        {
            _observerManager.Unsubscribe(channel);
            _currentMap.State.GameMap.ExitAsync(this.AsReference<IPlayer>());
            _currentMap.State.GameMap.UnsubscribeAsync(channel);

            return Task.CompletedTask;
        }

    }
}
