using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class DampingComponent : RangedComponent, IConvertible<Components.DampingComponent> {
    public void Convert(Components.DampingComponent component) =>
        component.Damping = FinalValue;
}
