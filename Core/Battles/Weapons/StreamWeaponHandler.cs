using System.Diagnostics;
using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Damage;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public abstract class StreamWeaponHandler : TankWeaponHandler, IStreamWeaponHandler, ITemperatureWeaponHandler {
    protected StreamWeaponHandler(BattleTank battleTank) : base(battleTank) {
        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(BattleConfigPath).CooldownIntervalSec);
        DamagePerSecond = ConfigManager.GetComponent<DamagePerSecondPropertyComponent>(MarketConfigPath).FinalValue;
    }

    Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

    public float DamagePerSecond { get; }
    public Dictionary<long, TimeSpan> IncarnationIdToHitTime { get; } = new();
    public Dictionary<long, TimeSpan> IncarnationIdToLastHitTime { get; } = new();

    public abstract float TemperatureLimit { get; }
    public abstract float TemperatureDelta { get; }
    public virtual TimeSpan TemperatureDuration => TimeSpan.Zero;

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId))
            return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        // todo update target temperature

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    protected bool IsCooldownActive(long targetIncarnationId) {
        if (!IncarnationIdToHitTime.TryGetValue(targetIncarnationId, out TimeSpan lastHitTime)) {
            IncarnationIdToLastHitTime.Remove(targetIncarnationId);
            IncarnationIdToHitTime[targetIncarnationId] = Stopwatch.Elapsed;
            return true;
        }

        if (Stopwatch.Elapsed - lastHitTime < Cooldown)
            return true;

        IncarnationIdToHitTime.Remove(targetIncarnationId);
        IncarnationIdToLastHitTime[targetIncarnationId] = lastHitTime;
        return false;
    }

    public TimeSpan GetTimeSinceLastHit(long incarnationId) =>
        IncarnationIdToLastHitTime.TryGetValue(incarnationId, out TimeSpan hitTime)
            ? Stopwatch.Elapsed - hitTime
            : TimeSpan.Zero;

    public virtual ValueTask Reset() =>
        BattleTank.BattlePlayer.PlayerConnection.Send(new StreamWeaponResetStateEvent(), BattleEntity);
}
