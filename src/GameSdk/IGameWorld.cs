using GameSdk.Base;
using GameSdk.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk
{
    public interface IGameWorld : IGrainWithIntegerKey, IGameTickable
    {
        Task<IEnumerable<IGameMap>> GetMapsAsync();

        Task<IGameMap> CreateMapAsync(MapData mapData);
        Task<bool> IsWorkingAsync();
    }
}
