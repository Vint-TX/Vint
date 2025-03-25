using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public class ExplosiveMassWeaponHandler(
    BattleTank tank,
    IDamageCalculator damageCalculator,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    float maxDamage,
    float minDamage
) : ModuleWeaponHandler(tank,
    damageCalculator,
    cooldown,
    marketEntity,
    battleEntity,
    false,
    int.MaxValue,
    int.MaxValue,
    int.MaxValue,
    maxDamage,
    minDamage,
    int.MaxValue), IDiscreteWeaponHandler {
    public override async Task Fire(HitTarget target, int targetIndex) {
        if (Exploded != null)
            await Exploded();

        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, ignoreSourceEffects: true);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public event Func<Task>? Exploded;
}
