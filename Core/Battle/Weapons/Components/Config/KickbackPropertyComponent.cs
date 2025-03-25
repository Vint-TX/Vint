using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class KickbackPropertyComponent : RangedComponent, IConvertible<KickbackComponent> {
    public void Convert(KickbackComponent component) =>
        component.KickbackForce = FinalValue;
}
