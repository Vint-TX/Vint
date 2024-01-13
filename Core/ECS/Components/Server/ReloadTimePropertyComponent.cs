using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class ReloadTimePropertyComponent : RangedComponent, IConvertible<WeaponCooldownComponent>, IConvertible<DiscreteWeaponEnergyComponent> {
    public void Convert(DiscreteWeaponEnergyComponent component) {
        component.ReloadEnergyPerSec = 1f / FinalValue;
        component.UnloadEnergyPerShot = 1f;
    }

    public void Convert(WeaponCooldownComponent component) =>
        component.CooldownIntervalSec = FinalValue;
}