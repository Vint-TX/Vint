using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl.EnergyInjection;

[ProtocolId(636367507221863506)]
public class EnergyInjectionModuleReloadEnergyComponent(
    float reloadEnergyPercent
) : IComponent {
    public float ReloadEnergyPercent { get; private set; } = reloadEnergyPercent;
}
