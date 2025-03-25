using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class AimingImpactPropertyComponent : RangedComponent, IConvertible<ShaftAimingImpactComponent> {
    public void Convert(ShaftAimingImpactComponent component) =>
        component.MaxImpactForce = FinalValue;
}
