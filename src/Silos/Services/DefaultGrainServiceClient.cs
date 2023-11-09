using GameSdk.Services;
using Orleans.Runtime.Services;
using Orleans.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silos.Services
{
    public sealed class DefaultGrainServiceClient<TService>(IServiceProvider serviceProvider) : GrainServiceClient<TService>(serviceProvider), IServiceClient<TService>
        where TService : IGrainService
    {
        public TService Service => GetGrainService(CurrentGrainReference.GrainId);
    }
}
