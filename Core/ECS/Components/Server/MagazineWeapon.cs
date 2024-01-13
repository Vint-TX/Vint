using Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;

namespace Vint.Core.ECS.Components.Server;

public class MagazineWeapon {
    public class ReloadMagazineTimePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent> {
        public void Convert(MagazineWeaponComponent component) =>
            component.ReloadMagazineTimePerSec = FinalValue;
    }

    public class MagazineSizePropertyComponent : RangedComponent, IConvertible<MagazineWeaponComponent>, IConvertible<MagazineStorageComponent> {
        public void Convert(MagazineStorageComponent component) =>
            component.CurrentCartridgeCount = (int)FinalValue;

        public void Convert(MagazineWeaponComponent component) =>
            component.MaxCartridgeCount = (int)FinalValue;
    }
}