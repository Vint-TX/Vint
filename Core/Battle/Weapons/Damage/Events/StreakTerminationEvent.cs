using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Damage.Events;

[ProtocolId(1512395506558)]
public class StreakTerminationEvent(
    string targetUsername
) : IEvent {
    public string VictimUid { get; } = targetUsername;
}
