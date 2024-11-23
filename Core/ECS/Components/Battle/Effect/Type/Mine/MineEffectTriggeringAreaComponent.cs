using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.Mine;

[ProtocolId(636377093029435859)]
public class MineEffectTriggeringAreaComponent(
    float radius
) : IComponent {
    public float Radius { get; } = radius;
}
