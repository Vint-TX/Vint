using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Splash;

[ProtocolId(3169143415222756957)]
public class SplashWeaponComponent(
    float minSplashDamagePercent,
    float radiusOfMaxSplashDamage,
    float radiusOfMinSplashDamage
) : IComponent {
    public float MinSplashDamagePercent { get; private set; } = minSplashDamagePercent;
    public float RadiusOfMaxSplashDamage { get; private set; } = radiusOfMaxSplashDamage;
    public float RadiusOfMinSplashDamage { get; private set; } = radiusOfMinSplashDamage;
}