using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class ReverseAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.ReverseAcceleration = FinalValue;
}
