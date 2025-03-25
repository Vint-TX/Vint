using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Damage.Components;

public class MaxDamageDistancePropertyComponent : RangedComponent, IConvertible<DamageWeakeningByDistanceComponent> {
    public void Convert(DamageWeakeningByDistanceComponent component) =>
        component.RadiusOfMaxDamage = FinalValue;
}
