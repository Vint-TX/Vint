using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Tank;

public class DampingComponent : RangedComponent, IConvertible<Components.Battle.Parameters.Chassis.DampingComponent> {
    public void Convert(Components.Battle.Parameters.Chassis.DampingComponent component) =>
        component.Damping = FinalValue;
}
