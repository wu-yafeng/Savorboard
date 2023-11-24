using Google.Protobuf.WellKnownTypes;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Protos
{
    public interface IAsyncNetwork
    {
        Task<Any?> PeekAsync();

        Task StartAsync(CancellationToken cancellationToken = default);

        Task<BackpackViewModel> GetBackpackDataViewModelAsync(GetBackpackViewModelPack messagePack);
    }
}
