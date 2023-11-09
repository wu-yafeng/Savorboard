using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Services
{
    public interface IMetaService : IGrainService
    {
        Task<T?> FindAsync<T>(int id) where T : IMetaItem;

        Task<IEnumerable<T>> GetAllAsync<T>() where T : IMetaItem;
    }
}
