using GameSdk.Services;
using Silos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos
{
    public static class MetaServiceExtensions
    {
        public static async Task<T> GetRequiredAsync<T>(this IMetaService service, int id) where T : IMetaItem
        {
            return await service.FindAsync<T>(id)
                ?? throw new BizException(ErrorCodes.MetaDataItemNotFound);
        }
    }
}
