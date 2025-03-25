using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Components;

[ProtocolId(-3961778961585441606)]
public class BonusRegionComponent(
    BonusType type
) : IComponent {
    public BonusType Type { get; private set; } = type;
}
