using System.Numerics;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl.Mine;

[ProtocolId(1431673085710)]
public class MinePositionComponent(
    Vector3 position
) : IComponent {
    public Vector3 Position { get; } = position;
}
