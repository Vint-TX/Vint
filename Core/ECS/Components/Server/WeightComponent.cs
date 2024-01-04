namespace Vint.Core.ECS.Components.Server;

public class WeightComponent : RangedComponent, IConvertible<Battle.Parameters.Chassis.WeightComponent> {
    public void Convert(Battle.Parameters.Chassis.WeightComponent component) =>
        component.Weight = FinalValue;
}