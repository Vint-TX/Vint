using System.Numerics;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Bonus.Type;

public class SpeedBox(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<TurboSpeedEffect>(battle, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Speed;

    protected override TurboSpeedEffect GetEffect(BattleTank battleTank) => new(battleTank);
}