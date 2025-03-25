using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

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
