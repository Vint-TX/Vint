using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(1543402751411)]
public class ExplosiveMassEffectComponent(
    float radius,
    float delay
) : IComponent {
    public float Radius { get; } = radius;
    public float Delay { get; } = delay;
}
