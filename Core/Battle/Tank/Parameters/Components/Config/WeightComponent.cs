using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Tank.Parameters.Components.Config;

public class WeightComponent : RangedComponent, IConvertible<Components.WeightComponent> {
    public void Convert(Components.WeightComponent component) =>
        component.Weight = FinalValue;
}
