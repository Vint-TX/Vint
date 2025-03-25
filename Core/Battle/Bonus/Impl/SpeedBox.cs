using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Battle.Bonus.Type;

public class SpeedBox(
    Round round,
    BonusInfo bonusInfo
) : SupplyBox<TurboSpeedEffect>(round, bonusInfo) {
    public override BonusType Type => BonusType.Speed;

    protected override TurboSpeedEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
