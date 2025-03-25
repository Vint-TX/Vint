using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl.EMP;

[ProtocolId(636250000933021510)]
public class EMPEffectComponent(
    float radius
) : IComponent {
    public float Radius { get; } = radius;
}
