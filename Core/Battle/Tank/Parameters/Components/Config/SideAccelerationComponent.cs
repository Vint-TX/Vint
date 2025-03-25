using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class SideAccelerationComponent : RangedComponent, IConvertible<SpeedConfigComponent> {
    public void Convert(SpeedConfigComponent component) =>
        component.SideAcceleration = FinalValue;
}
