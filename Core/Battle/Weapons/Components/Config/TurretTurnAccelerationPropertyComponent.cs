using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class TurretTurnAccelerationPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
    public void Convert(WeaponRotationComponent component) =>
        component.Acceleration = FinalValue;
}
