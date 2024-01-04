using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class ImpactPropertyComponent : RangedComponent, IConvertible<ImpactComponent> {
    public void Convert(ImpactComponent component) =>
        component.ImpactForce = FinalValue;
}