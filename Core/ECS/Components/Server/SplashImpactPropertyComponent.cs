using Vint.Core.ECS.Components.Battle.Weapon.Splash;

namespace Vint.Core.ECS.Components.Server;

public class SplashImpactPropertyComponent : RangedComponent, IConvertible<SplashImpactComponent> {
    public void Convert(SplashImpactComponent component) => component.ImpactForce = FinalValue;
}