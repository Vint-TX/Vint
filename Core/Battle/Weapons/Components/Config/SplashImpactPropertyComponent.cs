using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Server.Common;

namespace Vint.Core.ECS.Components.Server.Weapon;

public class SplashImpactPropertyComponent : RangedComponent, IConvertible<SplashImpactComponent> {
    public void Convert(SplashImpactComponent component) => component.ImpactForce = FinalValue;
}
