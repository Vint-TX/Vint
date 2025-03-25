using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class ImpactPropertyComponent : RangedComponent, IConvertible<ImpactComponent> {
    public void Convert(ImpactComponent component) =>
        component.ImpactForce = FinalValue;
}
