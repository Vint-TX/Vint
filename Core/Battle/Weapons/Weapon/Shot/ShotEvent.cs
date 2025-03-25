using System.Numerics;
using Vint.Core.Battle.Common.Events;

namespace Vint.Core.Battle.Weapons.Weapon.Shot;

public abstract class ShotEvent : TimeEvent {
    public Vector3? ShotDirection { get; set; }
    public int ShotId { get; set; }
}
