using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Damage;

public class MinDamageDistancePropertyComponent : RangedComponent, IConvertible<DamageWeakeningByDistanceComponent> {
    public void Convert(DamageWeakeningByDistanceComponent component) =>
        component.RadiusOfMinDamage = FinalValue;
}
