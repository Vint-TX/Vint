using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(1512395506558)]
public class StreakTerminationEvent(
    string targetUsername
) : IEvent {
    public string VictimUid { get; set; } = targetUsername;
}