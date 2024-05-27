using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class VulcanWeaponHandler : StreamWeaponHandler, IHeatWeaponHandler {
    public VulcanWeaponHandler(BattleTank battleTank) : base(battleTank) {
        OverheatingTime = TimeSpan.FromSeconds(ConfigManager.GetComponent<TemperatureHittingTimePropertyComponent>(MarketConfigPath).FinalValue);
        HeatDamage = ConfigManager.GetComponent<HeatDamagePropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta =
            ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * (float)Cooldown.TotalSeconds;
    }

    public override int MaxHitTargets => 1;

    public DateTimeOffset? ShootingStartTime { get; set; }
    public DateTimeOffset? LastOverheatingUpdate { get; set; }
    TimeSpan OverheatingTime { get; }

    bool IsOverheating => ShootingStartTime.HasValue &&
                          DateTimeOffset.UtcNow - ShootingStartTime >= OverheatingTime;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
    public float HeatDamage { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        // ReSharper disable once ArrangeRedundantParentheses
        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public override async Task Tick() {
        await base.Tick();
        await UpdateOverheating();
    }

    async Task UpdateOverheating() {
        if (!IsOverheating ||
            LastOverheatingUpdate.HasValue &&
            DateTimeOffset.UtcNow - LastOverheatingUpdate < Cooldown) return;

        if (BattleTank.StateManager.CurrentState is Dead) {
            ShootingStartTime = null;
            LastOverheatingUpdate = null;
            return;
        }

        await BattleTank.UpdateTemperatureAssists(BattleTank, this, false);
        LastOverheatingUpdate = DateTimeOffset.UtcNow;
    }

    public override ValueTask Reset() =>
        BattleTank.BattlePlayer.PlayerConnection.Send(new VulcanResetStateEvent(), BattleEntity);

    public override async Task OnTankDisable() {
        await base.OnTankDisable();

        ShootingStartTime = null;
        LastOverheatingUpdate = null;
    }
}
