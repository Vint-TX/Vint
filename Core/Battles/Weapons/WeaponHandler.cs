using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public abstract class WeaponHandler(
    BattleTank battleTank
) {
    public BattleTank BattleTank { get; } = battleTank;
}