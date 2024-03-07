using System.Numerics;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Bonus.Type;

public class ArmorBox(
    Battle battle,
    Vector3 regionPosition,
    bool hasParachute
) : SupplyBox<AbsorbingArmorEffect>(battle, regionPosition, hasParachute) {
    public override BonusType Type => BonusType.Armor;

    protected override AbsorbingArmorEffect GetEffect(BattleTank battleTank) => new(battleTank);
}