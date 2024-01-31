using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Flag;

[ProtocolId(-7424433796811681217)]
public class FlagPositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; set; } = position;
}