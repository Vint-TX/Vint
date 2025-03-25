using Vint.Core.Battle.Weapons.Components.Railgun;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class ChargeTimePropertyComponent : RangedComponent, IConvertible<RailgunChargingWeaponComponent> {
    public void Convert(RailgunChargingWeaponComponent component) =>
        component.ChargingTime = FinalValue;
}
