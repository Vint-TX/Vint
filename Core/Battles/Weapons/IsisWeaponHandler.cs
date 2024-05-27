using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class IsisWeaponHandler : StreamWeaponHandler {
    public IsisWeaponHandler(BattleTank battleTank) : base(battleTank) {
        HealPerSecond = ConfigManager.GetComponent<HealingPropertyComponent>(MarketConfigPath).FinalValue;
        SelfHealPercentage = ConfigManager.GetComponent<SelfHealingPropertyComponent>(MarketConfigPath).FinalValue;

        DecreaseFriendTemperature = ConfigManager.GetComponent<DecreaseFriendTemperaturePropertyComponent>(MarketConfigPath).FinalValue;
        IncreaseFriendTemperature = ConfigManager.GetComponent<IncreaseFriendTemperaturePropertyComponent>(MarketConfigPath).FinalValue;
    }

    public float HealPerSecond { get; }
    public float SelfHealPercentage { get; }

    public float DecreaseFriendTemperature { get; }
    public float IncreaseFriendTemperature { get; }

    public override int MaxHitTargets => 1;

    public override float TemperatureLimit => 0;
    public override float TemperatureDelta => 0;

    public override async Task Fire(HitTarget target, int targetIndex) {
        long incarnationId = target.IncarnationEntity.Id;

        if (IsCooldownActive(incarnationId)) return;

        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        if (targetTank.StateManager.CurrentState is not Active) return;

        bool isEnemy = BattleTank.IsEnemy(targetTank);
        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);

        if (isEnemy) {
            CalculatedDamage heal = damage with { Value = damage.Value / 100 * SelfHealPercentage };

            await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
            await battle.DamageProcessor.Heal(BattleTank, heal);
        } else {
            await targetTank.UpdateTemperatureAssists(BattleTank, this, true);

            const int healScore = 2;
            if (targetTank.Health >= targetTank.MaxHealth) return;

            await battle.DamageProcessor.Heal(BattleTank, targetTank, damage);

            int scoreWithBonus = BattleTank.BattlePlayer.GetScoreWithBonus(healScore);

            await BattleTank.AddScore(healScore);
            await BattleTank.CommitStatistics();
            await BattleTank.BattlePlayer.PlayerConnection.Send(new VisualScoreHealEvent(scoreWithBonus), BattleTank.BattleUser);
        }
    }
}
