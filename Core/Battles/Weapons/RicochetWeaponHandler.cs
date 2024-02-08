using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Weapons.Damage;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class RicochetWeaponHandler(
    BattleTank battleTank
) : DiscreteWeaponHandler(battleTank) {
    public override int MaxHitTargets => 1;

    public override void Fire(HitTarget target) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active ||
            (!isEnemy && !battle.Properties.FriendlyFire)) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, target);
        battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, damage);
    }
}