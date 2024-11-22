using JetBrains.Annotations;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Splash;

[ProtocolId(3169143415222756957)]
public class SplashWeaponComponent : IComponent {
    [UsedImplicitly]
    public SplashWeaponComponent() { }

    public SplashWeaponComponent(float minSplashDamagePercent, float radiusOfMaxSplashDamage, float radiusOfMinSplashDamage) {
        MinSplashDamagePercent = minSplashDamagePercent;
        RadiusOfMaxSplashDamage = radiusOfMaxSplashDamage;
        RadiusOfMinSplashDamage = radiusOfMinSplashDamage;
    }

    public float MinSplashDamagePercent { get; private set; }
    public float RadiusOfMaxSplashDamage { get; private set; }
    public float RadiusOfMinSplashDamage { get; private set; }
}
