using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class BulletWeapon {
    public class BulletRadiusPropertyComponent : RangedComponent, IConvertible<WeaponBulletShotComponent> {
        public void Convert(WeaponBulletShotComponent component) =>
            component.BulletRadius = FinalValue;
    }

    public class BulletSpeedPropertyComponent : RangedComponent, IConvertible<WeaponBulletShotComponent> {
        public void Convert(WeaponBulletShotComponent component) =>
            component.BulletSpeed = FinalValue;
    }
}