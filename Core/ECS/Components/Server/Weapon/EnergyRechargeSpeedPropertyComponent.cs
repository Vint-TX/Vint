using Vint.Core.ECS.Components.Battle.Weapon.Stream;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class EnergyRechargeSpeedPropertyComponent : RangedComponent, IConvertible<StreamWeaponEnergyComponent>, IConvertible<ShaftEnergyComponent> {
    public void Convert(ShaftEnergyComponent component) =>
        component.ReloadEnergyPerSec = FinalValue;

    public void Convert(StreamWeaponEnergyComponent component) =>
        component.ReloadEnergyPerSec = FinalValue;
}
