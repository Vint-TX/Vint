using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Flags.Components;

[ProtocolId(4898317045808451550)]
public class FlagPedestalComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; private set; } = position;
}
