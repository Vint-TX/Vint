using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(636367475685199712)]
public class EnergyInjectionEffectComponent(
    float reloadEnergyPercent
) : IComponent {
    public float ReloadEnergyPercent { get; private set; } = reloadEnergyPercent;
}
