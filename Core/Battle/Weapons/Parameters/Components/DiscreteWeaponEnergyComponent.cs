using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(1438077188268)]
public class DiscreteWeaponEnergyComponent(
    float reloadEnergyPerSec,
    float unloadEnergyPerShot
) : IComponent {
    public float ReloadEnergyPerSec { get; set; } = reloadEnergyPerSec;
    public float UnloadEnergyPerShot { get; set; } = unloadEnergyPerShot;
}
