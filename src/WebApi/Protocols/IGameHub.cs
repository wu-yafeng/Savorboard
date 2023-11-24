using GameSdk.ViewModels;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Channels;

namespace WebApi.Protocols
{
    public record SwitchToWorldPack(Guid WorldId);
    public interface IGameHub
    {
        Task<UBackpackViewModel> GetBackpackAsync();

        Task<ChannelReader<Any>> Subscribe(CancellationToken cancellationToken = default);
    }
}
