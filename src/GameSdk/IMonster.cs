using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSdk.Models;
using GameSdk.Observers;

namespace GameSdk
{
    public interface IMonster : IGameTickable, IGrainWithIntegerKey, ILifeBase
    {
        Task SetAttributes(UAttributeData attributes);

        Task SetMapAsync(IGameMap map);
    }
}
