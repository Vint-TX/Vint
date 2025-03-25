using Vint.Core.Battle.Tank;

namespace Vint.Core.Battle.Weapons.Handlers;

public abstract class BulletWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank);
