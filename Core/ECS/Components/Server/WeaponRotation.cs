using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class WeaponRotation {
    public class TurretTurnSpeedPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
        public void Convert(WeaponRotationComponent component) {
            component.BaseSpeed = FinalValue;
            component.Speed = FinalValue;
        }
    }

    public class TurretTurnAccelerationPropertyComponent : RangedComponent, IConvertible<WeaponRotationComponent> {
        public void Convert(WeaponRotationComponent component) =>
            component.Acceleration = FinalValue;
    }
}