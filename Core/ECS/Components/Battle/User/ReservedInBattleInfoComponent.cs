using Vint.Core.Battles;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.User;

[ProtocolId(1490682041080)]
public class ReservedInBattleInfoComponent(
    IEntity map,
    BattleMode mode,
    DateTimeOffset exitTime
) : IComponent {
    public long Map { get; private set; } = map.Id;
    public BattleMode BattleMode { get; private set; } = mode;
    public DateTimeOffset ExitTime { get; private set; } = exitTime;
}
