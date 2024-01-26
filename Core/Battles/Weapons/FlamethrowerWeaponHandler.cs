using Vint.Core.Battles.Player;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class FlamethrowerWeaponHandler(
    BattleTank battleTank
) : StreamWeaponHandler(battleTank) {
    public override void Fire(HitTarget target) {
        throw new NotImplementedException();
    }
}