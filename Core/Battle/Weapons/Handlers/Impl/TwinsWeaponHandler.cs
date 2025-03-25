using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Weapons;

public class TwinsWeaponHandler(
    BattleTank battleTank
) : BulletWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;
}
