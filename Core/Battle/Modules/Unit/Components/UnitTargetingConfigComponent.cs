using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Unit;

[ProtocolId(636364931473899150)]
public class UnitTargetingConfigComponent(
    float workDistance
) : IComponent {
    public float WorkDistance { get; } = workDistance;
}
