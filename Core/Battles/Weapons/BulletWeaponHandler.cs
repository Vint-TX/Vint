using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public abstract class BulletWeaponHandler : DiscreteWeaponHandler {
    protected BulletWeaponHandler(BattleTank battleTank) : base(battleTank) { }
}