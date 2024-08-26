using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.EMP;

[ProtocolId(636250000933021510)]
public class EMPEffectComponent(
    float radius
) : IComponent {
    public float Radius { get; } = radius;
}
