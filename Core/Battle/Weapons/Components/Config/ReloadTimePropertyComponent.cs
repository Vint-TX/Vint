using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class ReloadTimePropertyComponent : RangedComponent, IConvertible<WeaponCooldownComponent>, IConvertible<DiscreteWeaponEnergyComponent> {
    public void Convert(DiscreteWeaponEnergyComponent component) {
        component.ReloadEnergyPerSec = 1f / FinalValue;
        component.UnloadEnergyPerShot = 1f;
    }

    public void Convert(WeaponCooldownComponent component) =>
        component.CooldownIntervalSec = FinalValue;
}
