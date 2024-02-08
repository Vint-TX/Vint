using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public class RailgunWeaponHandler(
    BattleTank battleTank
) : DiscreteWeaponHandler(battleTank) {
    public override int MaxHitTargets => BattleTank.Battle.Players.Count;
}