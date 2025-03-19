using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.Temperature;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Weapon;
using Vint.Core.ECS.Events.Battle.Score.Visual;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using Vint.Core.ECS.Enums;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Weapons;

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

        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        if (targetTank.StateManager.CurrentState is not Active) return;

        bool isEnemy = BattleTank.IsEnemy(targetTank) && !BattleTank.IsSameTeam(targetTank) || (BattleTank.Round.Properties.BattleMode == BattleMode.DM);
        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);

        if (isEnemy) {
            CalculatedDamage heal = damage with { Value = damage.Value / 100 * SelfHealPercentage };

            await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
            await round.DamageProcessor.Heal(BattleTank, heal);
        } else {
            TemperatureAssist assist = TemperatureCalculator.Calculate(BattleTank, this, true);
            targetTank.TemperatureProcessor.EnqueueAssist(assist);

            const int healScore = 2;
            if (targetTank.Health >= targetTank.MaxHealth) return;

            await round.DamageProcessor.Heal(BattleTank, targetTank, damage);

            int scoreWithBonus = BattleTank.Tanker.GetScoreWithBonus(healScore);

            await BattleTank.AddScore(healScore);
            await BattleTank.CommitStatistics();
            await BattleTank.Tanker.Send(new VisualScoreHealEvent(scoreWithBonus), BattleTank.Tanker.BattleUser);
        }
    }
}
