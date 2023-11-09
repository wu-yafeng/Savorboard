using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk
{
    public interface ILifeBase : IGameObj
    {
        Task<bool> IsDeadAsync();
    }
}
