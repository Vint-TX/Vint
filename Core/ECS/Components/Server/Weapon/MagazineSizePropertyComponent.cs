using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class MagazineSizePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent>, IConvertible<MagazineStorageComponent> {
    public void Convert(MagazineStorageComponent component) =>
        component.CurrentCartridgeCount = (int)FinalValue;

    public void Convert(MagazineWeaponComponent component) =>
        component.MaxCartridgeCount = (int)FinalValue;
}
