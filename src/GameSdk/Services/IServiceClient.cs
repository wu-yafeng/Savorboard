using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSdk.Services
{
    public interface IServiceClient<TService> where TService : IGrainService
    {
        TService Service { get; }
    }
}
