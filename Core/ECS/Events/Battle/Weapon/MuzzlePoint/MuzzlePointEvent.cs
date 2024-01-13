namespace Vint.Core.ECS.Events.Battle.Weapon.MuzzlePoint;

public abstract class MuzzlePointEvent : IEvent {
    public int Index { get; set; }
}