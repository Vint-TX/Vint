using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Bonus;

[ProtocolId(4605414188335188027)]
public class PositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; private set; } = position;
}