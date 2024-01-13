using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(1826384779893027508)]
public class ShaftEnergyComponent(
    float unloadEnergyPerQuickShot,
    float possibleUnloadEnergyPerAimingShot,
    float unloadAimingEnergyPerSec,
    float reloadEnergyPerSec
) : IComponent {
    public float UnloadEnergyPerQuickShot { get; private set; } = unloadEnergyPerQuickShot;
    public float PossibleUnloadEnergyPerAimingShot { get; private set; } = possibleUnloadEnergyPerAimingShot;
    public float UnloadAimingEnergyPerSec { get; private set; } = unloadAimingEnergyPerSec;
    public float ReloadEnergyPerSec { get; private set; } = reloadEnergyPerSec;
}