using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Weapons;

public abstract class BulletWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank);