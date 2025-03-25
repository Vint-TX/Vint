using Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Damage;

public class DamageWeakeningByTargetPropertyComponent : RangedComponent, IConvertible<DamageWeakeningByTargetComponent> {
    public void Convert(DamageWeakeningByTargetComponent component) =>
        component.DamagePercent = FinalValue;
}
