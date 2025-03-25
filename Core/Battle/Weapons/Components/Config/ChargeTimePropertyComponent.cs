using Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class ChargeTimePropertyComponent : RangedComponent, IConvertible<RailgunChargingWeaponComponent> {
    public void Convert(RailgunChargingWeaponComponent component) =>
        component.ChargingTime = FinalValue;
}
