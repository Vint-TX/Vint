using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class SpeedComponent : RangedComponent, IConvertible<Components.SpeedComponent> {
    public void Convert(Components.SpeedComponent component) =>
        component.Speed = FinalValue;
}
