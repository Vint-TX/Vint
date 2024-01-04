using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(1438077188268)]
public class DiscreteWeaponEnergyComponent(
    float reloadEnergyPerSec,
    float unloadEnergyPerShot
) : IComponent {
    public float ReloadEnergyPerSec { get; set; } = reloadEnergyPerSec;
    public float UnloadEnergyPerShot { get; set; } = unloadEnergyPerShot;
}