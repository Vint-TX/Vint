using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Bonus.Type;

public class RepairBox(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<RepairKitEffect>(round, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Repair;

    protected override RepairKitEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
