using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Components;

[ProtocolId(1505906112954)]
public class LeagueFirstEntranceRewardNotificationComponent(
    Dictionary<IEntity, int> rewards
) : IComponent {
    public Dictionary<IEntity, int> Reward { get; private set; } = rewards;
}
