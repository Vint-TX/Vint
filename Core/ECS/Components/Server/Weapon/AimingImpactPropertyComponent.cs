using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class AimingImpactPropertyComponent : RangedComponent, IConvertible<ShaftAimingImpactComponent> {
    public void Convert(ShaftAimingImpactComponent component) =>
        component.MaxImpactForce = FinalValue;
}
