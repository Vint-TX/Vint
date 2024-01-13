using Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

namespace Vint.Core.ECS.Components.Server;

public class ShaftWeapon {
    public class AimingImpactPropertyComponent : RangedComponent, IConvertible<ShaftAimingImpactComponent> {
        public void Convert(ShaftAimingImpactComponent component) =>
            component.MaxImpactForce = FinalValue;
    }

    public class AimingHorizontalAccelerationPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
        public void Convert(ShaftAimingSpeedComponent component) =>
            component.HorizontalAcceleration = FinalValue;
    }

    public class AimingHorizontalSpeedPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
        public void Convert(ShaftAimingSpeedComponent component) =>
            component.MaxHorizontalSpeed = FinalValue;
    }

    public class AimingVerticalAccelerationPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
        public void Convert(ShaftAimingSpeedComponent component) =>
            component.VerticalAcceleration = FinalValue;
    }

    public class AimingVerticalSpeedPropertyComponent : RangedComponent, IConvertible<ShaftAimingSpeedComponent> {
        public void Convert(ShaftAimingSpeedComponent component) =>
            component.MaxVerticalSpeed = FinalValue;
    }

    public class ShaftStateConfig : RangedComponent, IConvertible<ShaftStateConfigComponent> {
        public void Convert(ShaftStateConfigComponent component) {
            component.WaitingToActivationTransitionTimeSec = FinalValue;
            component.ActivationToWorkingTransitionTimeSec = FinalValue;
            component.FinishToIdleTransitionTimeSec = FinalValue;
        }
    }
}