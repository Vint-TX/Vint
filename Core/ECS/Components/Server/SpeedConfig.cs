using Vint.Core.ECS.Components.Battle.Parameters.Chassis;

namespace Vint.Core.ECS.Components.Server;

public class TurnAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.TurnAcceleration = FinalValue;
}

public class SideAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.SideAcceleration = FinalValue;
}

public class ReverseAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.ReverseAcceleration = FinalValue;
}

public class ReverseTurnAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.ReverseTurnAcceleration = FinalValue;
}