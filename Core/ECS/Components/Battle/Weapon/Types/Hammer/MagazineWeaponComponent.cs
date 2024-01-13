using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;

[ProtocolId(4355651182908057733)]
public class MagazineWeaponComponent(
    int maxCartridgeCount,
    float reloadMagazineTimePerSec
) : IComponent {
    public int MaxCartridgeCount { get; set; } = maxCartridgeCount;
    public float ReloadMagazineTimePerSec { get; set; } = reloadMagazineTimePerSec;
}