using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.EnergyInjection;

[ProtocolId(636367475685199712)]
public class EnergyInjectionEffectComponent(
    float reloadEnergyPercent
) : IComponent {
    public float ReloadEnergyPercent { get; private set; } = reloadEnergyPercent;
}
