using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Mines;

public class Mine(
    IMineEffect effect
) : IEquatable<Mine>, IComparable<Mine> {
    IMineEffect Effect { get; } = effect;
    BattleTank Owner => Effect.Owner;
    Vector3 Position => Effect.Position;
    float TriggeringArea => Effect.TriggeringArea;
    int Index => Effect.Index;

    bool Triggered { get; set; }

    public int CompareTo(Mine? other) {
        if (other == null)
            return -1;

        return Owner == other.Owner
            ? Index.CompareTo(other.Index)
            : Owner
                .GetHashCode()
                .CompareTo(other.Owner.GetHashCode());
    }

    public bool Equals(Mine? other) =>
        other != null && Owner == other.Owner && Index == other.Index;

    public bool TryTrigger(BattleTank tank) {
        if (Triggered ||
            !tank.IsEnemy(Owner) ||
            Vector3.Distance(tank.Position, Position) > TriggeringArea)
            return false;

        Triggered = true;
        Effect.TryExplode();
        return true;
    }

    public override bool Equals(object? obj) =>
        obj is Mine mine && Equals(mine);

    public override int GetHashCode() => HashCode.Combine(Owner, Index);

    public static bool operator ==(Mine? lhs, Mine? rhs) =>
        (object?)lhs == null
            ? (object?)rhs == null
            : lhs.Equals(rhs);

    public static bool operator !=(Mine? lhs, Mine? rhs) => !(lhs == rhs);
}
