using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk
{
    public interface IGameObj
    {
        Task<int> GetObjTypeAsync();
    }
}
