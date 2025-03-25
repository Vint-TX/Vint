using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Battle.Bonus.Type;

public class RepairBox(
    Round round,
    BonusInfo bonusInfo
) : SupplyBox<RepairKitEffect>(round, bonusInfo) {
    public override BonusType Type => BonusType.Repair;

    protected override RepairKitEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
