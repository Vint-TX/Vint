using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Bonus;

[ProtocolId(-7944772313373733709)]
public class BonusDropTimeComponent(
    DateTimeOffset dropTime
) : IComponent {
    public DateTimeOffset DropTime { get; private set; } = dropTime;
}