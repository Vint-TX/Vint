using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class TurretTurnSpeedPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
    public void Convert(WeaponRotationComponent component) {
        component.BaseSpeed = FinalValue;
        component.Speed = FinalValue;
    }
}
