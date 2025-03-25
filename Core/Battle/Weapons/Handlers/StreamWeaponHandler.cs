using System.Diagnostics;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.State;
using Vint.Core.Battle.Tank.Temperature;
using Vint.Core.Battle.Weapons.Damage.Calculator;
using Vint.Core.Battle.Weapons.Damage.Components;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.Battle.Weapons.Weapon;
using Vint.Core.Battle.Weapons.Weapon.Hit;
using Vint.Core.Config;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Weapons.Handlers;

public abstract class StreamWeaponHandler : TankWeaponHandler, IStreamWeaponHandler, ITemperatureWeaponHandler {
    protected StreamWeaponHandler(BattleTank battleTank) : base(battleTank) {
        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(BattleConfigPath).CooldownIntervalSec);
        DamagePerSecond = ConfigManager.GetComponent<DamagePerSecondPropertyComponent>(MarketConfigPath).FinalValue;
    }

    Stopwatch Stopwatch { get; } = Stopwatch.StartNew();
    public Dictionary<long, TimeSpan> IncarnationIdToHitTime { get; } = new();
    public Dictionary<long, TimeSpan> IncarnationIdToLastHitTime { get; } = new();

    public float DamagePerSecond { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId))
            return;

        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        TemperatureAssist assist = TemperatureCalculator.Calculate(BattleTank, this, !isEnemy);
        targetTank.TemperatureProcessor.EnqueueAssist(assist);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public TimeSpan GetTimeSinceLastHit(long incarnationId) =>
        IncarnationIdToLastHitTime.TryGetValue(incarnationId, out TimeSpan hitTime)
            ? Stopwatch.Elapsed - hitTime
            : TimeSpan.Zero;

    public abstract float TemperatureLimit { get; }
    public abstract float TemperatureDelta { get; }
    public virtual TimeSpan TemperatureDuration => TimeSpan.Zero;

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

    public virtual Task Reset() =>
        BattleTank.Tanker.Send<StreamWeaponResetStateEvent>(BattleEntity);
}
