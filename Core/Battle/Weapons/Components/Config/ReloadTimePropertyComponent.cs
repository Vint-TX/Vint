using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class ReloadTimePropertyComponent : RangedComponent, IConvertible<WeaponCooldownComponent>, IConvertible<DiscreteWeaponEnergyComponent> {
    public void Convert(DiscreteWeaponEnergyComponent component) {
        component.ReloadEnergyPerSec = 1f / FinalValue;
        component.UnloadEnergyPerShot = 1f;
    }

    public void Convert(WeaponCooldownComponent component) =>
        component.CooldownIntervalSec = FinalValue;
}
