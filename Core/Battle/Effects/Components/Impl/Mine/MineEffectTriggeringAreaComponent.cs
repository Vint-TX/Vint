using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl.Mine;

[ProtocolId(636377093029435859)]
public class MineEffectTriggeringAreaComponent(
    float radius
) : IComponent {
    public float Radius { get; } = radius;
}
