using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl;

[ProtocolId(1543402751411)]
public class ExplosiveMassEffectComponent(
    float radius,
    float delay
) : IComponent {
    public float Radius { get; } = radius;
    public float Delay { get; } = delay;
}
