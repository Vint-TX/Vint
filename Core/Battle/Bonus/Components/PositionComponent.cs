using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Components;

[ProtocolId(4605414188335188027)]
public class PositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; private set; } = position;
}
