using System.Numerics;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Bonus.Type;

public class RepairBox(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<RepairKitEffect>(battle, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Repair;

    protected override RepairKitEffect GetEffect(BattleTank battleTank) => new(battleTank);
}