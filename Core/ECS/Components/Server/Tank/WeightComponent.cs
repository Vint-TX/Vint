using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Tank;

public class WeightComponent : RangedComponent, IConvertible<Components.Battle.Parameters.Chassis.WeightComponent> {
    public void Convert(Components.Battle.Parameters.Chassis.WeightComponent component) =>
        component.Weight = FinalValue;
}
