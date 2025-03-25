using Vint.Core.Battle.Weapons.Components.Hammer;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class MagazineSizePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent>, IConvertible<MagazineStorageComponent> {
    public void Convert(MagazineStorageComponent component) =>
        component.CurrentCartridgeCount = (int)FinalValue;

    public void Convert(MagazineWeaponComponent component) =>
        component.MaxCartridgeCount = (int)FinalValue;
}
