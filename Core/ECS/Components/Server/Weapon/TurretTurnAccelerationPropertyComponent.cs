using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class TurretTurnAccelerationPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
    public void Convert(WeaponRotationComponent component) =>
        component.Acceleration = FinalValue;
}
