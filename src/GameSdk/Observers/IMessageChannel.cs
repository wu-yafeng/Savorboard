using GameSdk.Models;
using GameSdk.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Observers
{
    public interface IMessageChannel : IPlayerObserver, IMonsterObserver, IGameMapObserver
    {

    }
}
