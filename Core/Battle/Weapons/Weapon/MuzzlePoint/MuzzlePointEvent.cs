using Vint.Core.ECS.Events;

namespace Vint.Core.Battle.Weapons.Weapon.MuzzlePoint;

public abstract class MuzzlePointEvent : IEvent {
    public required int Index { get; init; }
}
