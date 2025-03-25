using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Hammer;

[ProtocolId(4355651182908057733)]
public class MagazineWeaponComponent(
    int maxCartridgeCount,
    float reloadMagazineTimePerSec
) : IComponent {
    public int MaxCartridgeCount { get; set; } = maxCartridgeCount;
    public float ReloadMagazineTimePerSec { get; set; } = reloadMagazineTimePerSec;
}
