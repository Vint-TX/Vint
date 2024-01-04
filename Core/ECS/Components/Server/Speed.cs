namespace Vint.Core.ECS.Components.Server;

public class Speed {
    public class SpeedComponent : RangedComponent, IConvertible<Battle.Parameters.Chassis.SpeedComponent> {
        public void Convert(Battle.Parameters.Chassis.SpeedComponent component) =>
            component.Speed = FinalValue;
    }

    public class TurnSpeedComponent : RangedComponent, IConvertible<Battle.Parameters.Chassis.SpeedComponent> {
        public void Convert(Battle.Parameters.Chassis.SpeedComponent component) =>
            component.TurnSpeed = FinalValue;
    }

    public class AccelerationComponent : RangedComponent, IConvertible<Battle.Parameters.Chassis.SpeedComponent> {
        public void Convert(Battle.Parameters.Chassis.SpeedComponent component) =>
            component.Acceleration = FinalValue;
    }
}