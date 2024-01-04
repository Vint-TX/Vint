using Vint.Core.ECS.Components.Battle.Weapon;

namespace Vint.Core.ECS.Components.Server;

public class KickbackPropertyComponent : RangedComponent, IConvertible<KickbackComponent> {
    public void Convert(KickbackComponent component) =>
        component.KickbackForce = FinalValue;
}