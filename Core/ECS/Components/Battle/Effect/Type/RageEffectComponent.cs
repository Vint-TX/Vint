using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(636364996704090103)]
public class RageEffectComponent(
    int decreaseCooldownPerKillMs
) : IComponent {
    public RageEffectComponent(TimeSpan decreaseCooldownPerKill) : this((int)Math.Ceiling(decreaseCooldownPerKill.TotalMilliseconds)) { }
    
    public int DecreaseCooldownPerKillMs { get; private set; } = decreaseCooldownPerKillMs;
}