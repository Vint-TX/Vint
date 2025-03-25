using Vint.Core.ECS.Events;

namespace Vint.Core.Battle.Weapons.Weapon.Hit;

public abstract class UpdateStreamHitEvent : IEvent {
    public StaticHit? StaticHit { get; set; }
    public HitTarget? TankHit { get; set; }
}
