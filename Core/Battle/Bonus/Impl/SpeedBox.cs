using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Battle.Bonus.Impl;

public class SpeedBox(
    Round round,
    BonusInfo bonusInfo
) : SupplyBox<TurboSpeedEffect>(round, bonusInfo) {
    public override BonusType Type => BonusType.Speed;

    protected override TurboSpeedEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
