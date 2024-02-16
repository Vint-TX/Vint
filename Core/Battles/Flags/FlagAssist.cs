using System.Numerics;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Flags;

public record struct FlagAssist(
    BattleTank Player,
    Vector3 LastPickupPoint,
    float TraveledDistance = 0
) {
    public readonly bool Equals(FlagAssist other) => Player.Equals(other.Player);

    public readonly override int GetHashCode() => Player.GetHashCode();
}