namespace Vint.Core.ECS.Events.Battle.Weapon.MuzzlePoint;

public abstract class MuzzlePointEvent : IEvent {
    public required int Index { get; init; }
}
