using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Bonus.Type;

public class ArmorBox(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<AbsorbingArmorEffect>(round, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Armor;

    protected override AbsorbingArmorEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
