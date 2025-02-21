using System.Numerics;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Flags;

public class FlagAssist(
    BattleTank tank,
    Vector3 pickupPoint
) {
    public float TraveledDistance { get; set; }
    public Vector3 LastPickupPoint { get; set; } = pickupPoint;
    public BattleTank Tank { get; } = tank;

    public override bool Equals(object? obj) => obj is BattleTank other && other == Tank;

    public override int GetHashCode() => Tank.GetHashCode();
}
