using System.Numerics;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot;

public abstract class ShotEvent : TimeEvent {
    public Vector3? ShotDirection { get; set; }
    public int ShotId { get; set; }
}