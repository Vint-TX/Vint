using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class KickbackPropertyComponent : RangedComponent, IConvertible<KickbackComponent> {
    public void Convert(KickbackComponent component) =>
        component.KickbackForce = FinalValue;
}
