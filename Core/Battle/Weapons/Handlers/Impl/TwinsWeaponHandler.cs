using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Weapons.Handlers.Impl;

public class TwinsWeaponHandler(
    BattleTank battleTank
) : BulletWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;
}
