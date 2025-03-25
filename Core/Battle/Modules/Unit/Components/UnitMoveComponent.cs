using System.Numerics;
using Vint.Core.Battle.Tank.Movement;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Unit.Components;

[ProtocolId(1485519196443)]
public class UnitMoveComponent(
    Vector3 position,
    Quaternion rotation
) : IComponent {
    public Movement Movement { get; set; } = new() {
        Position = position,
        Orientation = rotation
    };
}
