using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Battle.Bonus.Impl;

public class ArmorBox(
    Round round,
    BonusInfo bonusInfo
) : SupplyBox<AbsorbingArmorEffect>(round, bonusInfo) {
    public override BonusType Type => BonusType.Armor;

    protected override AbsorbingArmorEffect GetEffect(BattleTank battleTank) => new(battleTank);
}
