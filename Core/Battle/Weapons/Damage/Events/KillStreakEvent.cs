using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Damage.Events;

[ProtocolId(1491556721814)]
public class KillStreakEvent(
    int score
) : IEvent {
    public int Score { get; private set; } = score;
}
