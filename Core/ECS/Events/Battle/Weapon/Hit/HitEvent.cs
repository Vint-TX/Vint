using System.Numerics;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

public abstract class HitEvent : TimeEvent {
    public List<HitTarget>? Targets { get; set; }
    public StaticHit? StaticHit { get; set; }
    public int ShotId { get; set; }
}

public class StaticHit {
    public Vector3 Normal { get; private set; }
    public Vector3 Position { get; private set; }
}

public class HitTarget {
    public IEntity Entity { get; private set; } = null!;
    public IEntity IncarnationEntity { get; private set; } = null!;
    public Vector3 LocalHitPoint { get; private set; }
    public Vector3 TargetPosition { get; private set; }
    public Vector3 HitDirection { get; private set; }
    public float HitDistance { get; private set; }
}