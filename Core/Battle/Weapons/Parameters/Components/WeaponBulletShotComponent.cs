using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(1438152738643)]
public class WeaponBulletShotComponent(
    float bulletRadius,
    float bulletSpeed
) : IComponent {
    public float BulletRadius { get; set; } = bulletRadius;
    public float BulletSpeed { get; set; } = bulletSpeed;
}
