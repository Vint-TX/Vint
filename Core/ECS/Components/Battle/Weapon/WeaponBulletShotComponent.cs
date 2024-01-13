using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(1438152738643)]
public class WeaponBulletShotComponent(
    float bulletRadius,
    float bulletSpeed
) : IComponent {
    public float BulletRadius { get; set; } = bulletRadius;
    public float BulletSpeed { get; set; } = bulletSpeed;
}