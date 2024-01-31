using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Flag;

[ProtocolId(4898317045808451550)]
public class FlagPedestalComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; private set; } = position;
}