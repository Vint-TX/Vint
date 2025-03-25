using Vint.Core.Battle.Weapons.Components.Hammer;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class ReloadMagazineTimePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent> {
    public void Convert(MagazineWeaponComponent component) =>
        component.ReloadMagazineTimePerSec = FinalValue;
}
