using GameSdk.ViewModels;

namespace WebApi.Protocols
{
    public record SwitchToWorldPack(Guid WorldId);
    public interface IGameHub
    {
        Task<UBackpackViewModel> GetBackpackAsync();
        Task HeartbeatAsync();
    }
}
