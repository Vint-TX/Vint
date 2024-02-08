using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public class TwinsWeaponHandler(
    BattleTank battleTank
) : BulletWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;
}