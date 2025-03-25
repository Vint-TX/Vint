using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Stream;

[ProtocolId(1438077278464)]
public class StreamWeaponEnergyComponent(
    float reloadEnergyPerSec,
    float unloadEnergyPerSec
) : IComponent {
    public float ReloadEnergyPerSec { get; set; } = reloadEnergyPerSec;
    public float UnloadEnergyPerSec { get; set; } = unloadEnergyPerSec;
}
