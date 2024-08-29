using System.Diagnostics;
using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class DroneWeaponHandler(
    BattleTank tank,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    float damage
) : ModuleWeaponHandler(tank, cooldown, marketEntity, battleEntity, false, 0, 0, 0, damage, damage, 1), IStreamWeaponHandler {
    long _incarnationId;
    Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
    TimeSpan HitTime { get; set; }
    TimeSpan LastHitTime { get; set; }

    public long IncarnationId {
        get => _incarnationId;
        set {
            _incarnationId = value;

            HitTime = TimeSpan.MinValue;
            LastHitTime = TimeSpan.MinValue;
        }
    }

    public float DamagePerSecond { get; } = damage;

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IncarnationId != incarnationId || IsCooldownActive())
            return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, ignoreSourceEffects: true);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

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

    public TimeSpan GetTimeSinceLastHit(long incarnationId) =>
        incarnationId == IncarnationId && LastHitTime != TimeSpan.MinValue
            ? Stopwatch.Elapsed - LastHitTime
            : TimeSpan.Zero;
}
