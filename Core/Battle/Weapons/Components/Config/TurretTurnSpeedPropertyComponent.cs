using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class TurretTurnSpeedPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
    public void Convert(WeaponRotationComponent component) {
        component.BaseSpeed = FinalValue;
        component.Speed = FinalValue;
    }
}
