using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Flags.Components;

[ProtocolId(-7424433796811681217)]
public class FlagPositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; set; } = position;
}
