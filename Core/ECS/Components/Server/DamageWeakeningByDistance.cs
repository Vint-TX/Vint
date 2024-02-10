using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class MinDamagePercentPropertyComponent : RangedComponent, IConvertible<DamageWeakeningByDistanceComponent> {
    public void Convert(DamageWeakeningByDistanceComponent component) =>
        component.MinDamagePercent = FinalValue;
}

public class MinDamageDistancePropertyComponent : RangedComponent, IConvertible<DamageWeakeningByDistanceComponent> {
    public void Convert(DamageWeakeningByDistanceComponent component) =>
        component.RadiusOfMinDamage = FinalValue;
}

public class MaxDamageDistancePropertyComponent : RangedComponent, IConvertible<DamageWeakeningByDistanceComponent> {
    public void Convert(DamageWeakeningByDistanceComponent component) =>
        component.RadiusOfMaxDamage = FinalValue;
}