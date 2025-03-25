using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class ReloadMagazineTimePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent> {
    public void Convert(MagazineWeaponComponent component) =>
        component.ReloadMagazineTimePerSec = FinalValue;
}
