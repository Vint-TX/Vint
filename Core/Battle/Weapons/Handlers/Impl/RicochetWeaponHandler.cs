using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.State;
using Vint.Core.Battle.Weapons.Damage.Calculator;
using Vint.Core.Battle.Weapons.Weapon.Hit;

namespace Vint.Core.Battle.Weapons.Handlers.Impl;

public class RicochetWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;

    public override async Task Fire(HitTarget target, int targetIndex) {
        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
