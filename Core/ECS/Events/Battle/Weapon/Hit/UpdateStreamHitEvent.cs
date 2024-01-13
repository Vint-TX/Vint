namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

public abstract class UpdateStreamHitEvent : IEvent {
    public StaticHit? StaticHit { get; set; }
    public HitTarget? TankHit { get; set; }
}