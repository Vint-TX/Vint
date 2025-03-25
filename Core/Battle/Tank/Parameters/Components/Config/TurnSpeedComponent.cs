using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class TurnSpeedComponent : RangedComponent, IConvertible<Components.SpeedComponent> {
    public void Convert(Components.SpeedComponent component) =>
        component.TurnSpeed = FinalValue;
}
