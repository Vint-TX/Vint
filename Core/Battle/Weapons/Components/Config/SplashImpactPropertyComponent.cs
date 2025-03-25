using Vint.Core.Battle.Weapons.Components.Splash;
using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Weapons.Components.Config;

public class SplashImpactPropertyComponent : RangedComponent, IConvertible<SplashImpactComponent> {
    public void Convert(SplashImpactComponent component) => component.ImpactForce = FinalValue;
}
