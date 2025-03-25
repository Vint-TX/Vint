using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class BulletSpeedPropertyComponent : RangedComponent, IConvertible<WeaponBulletShotComponent> {
    public void Convert(WeaponBulletShotComponent component) =>
        component.BulletSpeed = FinalValue;
}
