using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Components;

[ProtocolId(-7944772313373733709)]
public class BonusDropTimeComponent(
    DateTimeOffset dropTime
) : IComponent {
    public DateTimeOffset DropTime { get; private set; } = dropTime;
}
