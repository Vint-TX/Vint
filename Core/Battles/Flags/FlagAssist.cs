using System.Numerics;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Flags;

public record struct FlagAssist(
    BattleTank Player,
    Vector3 LastPickupPoint,
    float TraveledDistance = 0
);