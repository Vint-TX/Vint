using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Battle.Bonus.Type;

public class DamageBox(
    Round round,
    BonusInfo bonusInfo
) : SupplyBox<IncreasedDamageEffect>(round, bonusInfo) {
    public override BonusType Type => BonusType.Damage;

    protected override IncreasedDamageEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
