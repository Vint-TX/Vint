using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(1491556721814)]
public class KillStreakEvent(
    int score
) : IEvent {
    public int Score { get; private set; } = score;
}