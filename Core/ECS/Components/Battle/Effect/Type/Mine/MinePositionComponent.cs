using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.Mine;

[ProtocolId(1431673085710)]
public class MinePositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; } = position;
}
