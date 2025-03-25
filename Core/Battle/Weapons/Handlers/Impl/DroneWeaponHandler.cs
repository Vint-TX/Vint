using System.Diagnostics;
using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public class DroneWeaponHandler(
    BattleTank tank,
    IDamageCalculator damageCalculator,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    float damage
) : ModuleWeaponHandler(tank, damageCalculator, cooldown, marketEntity, battleEntity, false, 0, 0, 0, damage, damage, 1), IStreamWeaponHandler {
    Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
    TimeSpan HitTime { get; set; }
    TimeSpan LastHitTime { get; set; }

    public long IncarnationId {
        get;
        set {
            field = value;

            HitTime = TimeSpan.MinValue;
            LastHitTime = TimeSpan.MinValue;
        }
    }

    public float DamagePerSecond { get; } = damage;

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IncarnationId != incarnationId || IsCooldownActive())
            return;

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

    public TimeSpan GetTimeSinceLastHit(long incarnationId) =>
        incarnationId == IncarnationId && LastHitTime != TimeSpan.MinValue
            ? Stopwatch.Elapsed - LastHitTime
            : TimeSpan.Zero;

    bool IsCooldownActive() {
        if (HitTime == TimeSpan.MinValue) {
            HitTime = Stopwatch.Elapsed;
            LastHitTime = TimeSpan.MinValue;
            return true;
        }

        if (Stopwatch.Elapsed - HitTime < Cooldown)
            return true;

        LastHitTime = HitTime;
        HitTime = TimeSpan.MinValue;
        return false;
    }
}
