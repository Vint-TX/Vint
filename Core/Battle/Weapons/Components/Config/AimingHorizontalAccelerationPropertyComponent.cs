using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class AimingHorizontalAccelerationPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
    public void Convert(ShaftAimingSpeedComponent component) =>
        component.HorizontalAcceleration = FinalValue;
}
