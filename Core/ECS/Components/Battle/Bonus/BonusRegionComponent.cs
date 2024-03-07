using Vint.Core.Battles.Bonus;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Bonus;

[ProtocolId(-3961778961585441606)]
public class BonusRegionComponent(
    BonusType type
) : IComponent {
    public BonusType Type { get; private set; } = type;
}