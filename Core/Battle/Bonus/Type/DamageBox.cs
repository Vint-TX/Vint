using System.Numerics;
using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Bonus.Type;

public class DamageBox(
    Round round,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<IncreasedDamageEffect>(round, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Damage;

    protected override IncreasedDamageEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
