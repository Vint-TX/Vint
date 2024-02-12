using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(636147227222284613)]
public class UserRankRewardNotificationInfoComponent(
    long rank,
    long crystals,
    long xCrystals
) : IComponent {
    public long Rank { get; private set; } = rank;
    public long BlueCrystals { get; private set; } = crystals;
    public long RedCrystals { get; private set; } = xCrystals;
}