using Vint.Core.Battle.Weapons.Components.Railgun;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Damage.Components;

public class DamageWeakeningByTargetPropertyComponent : RangedComponent, IConvertible<DamageWeakeningByTargetComponent> {
    public void Convert(DamageWeakeningByTargetComponent component) =>
        component.DamagePercent = FinalValue;
}
