using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(1438077278464)]
public class StreamWeaponEnergyComponent(
    float reloadEnergyPerSec,
    float unloadEnergyPerSec
) : IComponent {
    public float ReloadEnergyPerSec { get; set; } = reloadEnergyPerSec;
    public float UnloadEnergyPerSec { get; set; } = unloadEnergyPerSec;
}