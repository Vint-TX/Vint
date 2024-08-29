using Vint.Core.Battles.Tank;

namespace Vint.Core.Battles.Weapons;

public abstract class BulletWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank);
