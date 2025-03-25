using Vint.Core.Battle.Weapons.Components.Shaft;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class AimingVerticalAccelerationPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
    public void Convert(ShaftAimingSpeedComponent component) =>
        component.VerticalAcceleration = FinalValue;
}
