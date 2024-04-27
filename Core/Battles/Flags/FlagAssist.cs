using System.Numerics;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Flags;

public sealed record FlagAssist(
    BattleTank Tank
) {
    public bool Equals(FlagAssist? other) => Tank.Equals(other?.Tank);

    public override int GetHashCode() => Tank.GetHashCode();
    
    public float TraveledDistance { get; set; }
    public required Vector3 LastPickupPoint { get; set; }
}