using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Bonus.Type;

public class SpeedBox(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<TurboSpeedEffect>(round, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Speed;

    protected override TurboSpeedEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
