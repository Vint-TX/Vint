using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class AccelerationComponent : RangedComponent, IConvertible<Components.SpeedComponent> {
    public void Convert(Components.SpeedComponent component) =>
        component.Acceleration = FinalValue;
}
