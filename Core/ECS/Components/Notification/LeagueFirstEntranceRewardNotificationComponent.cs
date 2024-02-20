using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1505906112954)]
public class LeagueFirstEntranceRewardNotificationComponent(
    Dictionary<IEntity, int> rewards
) : IComponent {
    public Dictionary<IEntity, int> Reward { get; private set; } = rewards;
}