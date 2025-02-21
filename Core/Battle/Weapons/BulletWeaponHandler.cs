using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Weapons;

public abstract class BulletWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank);
