using System.Numerics;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Bonus.Type;

public class DamageBox(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<IncreasedDamageEffect>(battle, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Damage;

    protected override IncreasedDamageEffect GetEffect(BattleTank battleTank) => new(battleTank);
}