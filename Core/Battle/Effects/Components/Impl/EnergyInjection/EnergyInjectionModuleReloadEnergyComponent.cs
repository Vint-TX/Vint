using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type.EnergyInjection;

[ProtocolId(636367507221863506)]
public class EnergyInjectionModuleReloadEnergyComponent(
    float reloadEnergyPercent
) : IComponent {
    public float ReloadEnergyPercent { get; private set; } = reloadEnergyPercent;
}
