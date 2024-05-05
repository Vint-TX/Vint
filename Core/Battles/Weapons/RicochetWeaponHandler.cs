using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class RicochetWeaponHandler(
    BattleTank battleTank
) : DiscreteTankWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;

    public override async Task Fire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
