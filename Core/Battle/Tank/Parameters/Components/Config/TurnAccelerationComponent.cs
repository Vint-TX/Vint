using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class TurnAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.TurnAcceleration = FinalValue;
}
