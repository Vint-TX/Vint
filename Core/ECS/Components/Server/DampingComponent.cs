namespace Vint.Core.ECS.Components.Server;

public class DampingComponent : RangedComponent, IConvertible<Battle.Parameters.Chassis.DampingComponent> {
    public void Convert(Battle.Parameters.Chassis.DampingComponent component) =>
        component.Damping = FinalValue;
}