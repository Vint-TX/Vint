using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public class FreezeWeaponHandler(
    BattleTank battleTank
) : StreamWeaponHandler(battleTank) {
    public override int MaxHitTargets => int.MaxValue;
}