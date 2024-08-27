using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Unit;

[ProtocolId(1485519196443)]
public class UnitMoveComponent(
    Vector3 position,
    Quaternion rotation
) : IComponent {
    public ECS.Movement.Movement Movement { get; set; } = new() {
        Position = position,
        Orientation = rotation
    };
}
