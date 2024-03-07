using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Bonus;

[ProtocolId(8960819779144518)]
public class SpatialGeometryComponent(
    Vector3 position,
    Vector3 rotation
) : IComponent {
    public Vector3 Position { get; private set; } = position;
    public Vector3 Rotation { get; private set; } = rotation;
}