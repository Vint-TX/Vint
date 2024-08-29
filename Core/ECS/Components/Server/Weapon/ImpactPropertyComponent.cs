using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class ImpactPropertyComponent : RangedComponent, IConvertible<ImpactComponent> {
    public void Convert(ImpactComponent component) =>
        component.ImpactForce = FinalValue;
}
