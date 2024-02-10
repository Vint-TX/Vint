using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(1826384779893027508)]
public class ShaftEnergyComponent(
    float unloadEnergyPerQuickShot,
    float unloadAimingEnergyPerSec,
    float reloadEnergyPerSec,
    float possibleUnloadEnergyPerAimingShot = 1f
) : IComponent {
    public float UnloadEnergyPerQuickShot { get; set; } = unloadEnergyPerQuickShot;
    public float PossibleUnloadEnergyPerAimingShot { get; set; } = possibleUnloadEnergyPerAimingShot;
    public float UnloadAimingEnergyPerSec { get; set; } = unloadAimingEnergyPerSec;
    public float ReloadEnergyPerSec { get; set; } = reloadEnergyPerSec;
}