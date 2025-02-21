using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Temperature;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public class VulcanWeaponHandler : StreamWeaponHandler, IHeatWeaponHandler {
    public VulcanWeaponHandler(BattleTank battleTank) : base(battleTank) {
        OverheatingTime = TimeSpan.FromSeconds(ConfigManager.GetComponent<TemperatureHittingTimePropertyComponent>(MarketConfigPath).FinalValue);
        HeatDamage = ConfigManager.GetComponent<HeatDamagePropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta = (float)(ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * Cooldown.TotalSeconds);
    }

    public override int MaxHitTargets => 1;

    public DateTimeOffset? ShootingStartTime { get; set; }
    public DateTimeOffset? LastOverheatingUpdate { get; set; }
    TimeSpan OverheatingTime { get; }

    bool IsOverheating => ShootingStartTime.HasValue && DateTimeOffset.UtcNow - ShootingStartTime >= OverheatingTime;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
    public float HeatDamage { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);
        UpdateOverheating();
    }

    void UpdateOverheating() {
        if (!IsOverheating ||
            LastOverheatingUpdate.HasValue && DateTimeOffset.UtcNow - LastOverheatingUpdate < Cooldown) return;

        if (BattleTank.StateManager.CurrentState is Dead) {
            ShootingStartTime = null;
            LastOverheatingUpdate = null;
            return;
        }

        TemperatureAssist assist = TemperatureCalculator.Calculate(BattleTank, this, false);
        BattleTank.TemperatureProcessor.EnqueueAssist(assist);

        LastOverheatingUpdate = DateTimeOffset.UtcNow;
    }

    public override Task Reset() =>
        BattleTank.Tanker.Connection.Send(new VulcanResetStateEvent(), BattleEntity);

    public override async Task OnTankDisable() {
        await base.OnTankDisable();

        ShootingStartTime = null;
        LastOverheatingUpdate = null;
    }
}
