using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class ExplosiveMassWeaponHandler(
    BattleTank tank,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    float maxDamage,
    float minDamage
) : ModuleWeaponHandler(
    tank,
    cooldown,
    marketEntity,
    battleEntity,
    false,
    int.MaxValue,
    int.MaxValue,
    int.MaxValue,
    maxDamage,
    minDamage,
    int.MaxValue
), IDiscreteWeaponHandler {
    public event Func<Task>? Exploded;

    public override async Task Fire(HitTarget target, int targetIndex) {
        if (Exploded != null)
            await Exploded();

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, ignoreSourceEffects: true);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
