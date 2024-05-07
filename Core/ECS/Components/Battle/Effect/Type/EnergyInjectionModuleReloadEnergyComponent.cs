using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(636367507221863506)]
public class EnergyInjectionModuleReloadEnergyComponent(
    float reloadEnergyPercent
) : IComponent {
    public float ReloadEnergyPercent { get; private set; } = reloadEnergyPercent;
}
