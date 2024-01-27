using Vint.Core.Battles.Player;
using Vint.Core.Battles.States;
using Vint.Core.Battles.Weapons.Damage;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class ThunderWeaponHandler(
    BattleTank battleTank
) : DiscreteWeaponHandler(battleTank) {
    public void SplashFire(HitTarget target) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active ||
            (!isEnemy && !battle.Properties.FriendlyFire)) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, target, true);
        battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, damage);
    }

    public float GetSplashMultiplier(float distance) {
        if (distance < MaxDamageDistance) return 1;
        if (distance > MinDamageDistance) return 0;

        return 0.01f * (MinDamagePercent + (MinDamageDistance - distance) * (100f - MinDamagePercent) / (MinDamageDistance - MaxDamageDistance));
    }
}