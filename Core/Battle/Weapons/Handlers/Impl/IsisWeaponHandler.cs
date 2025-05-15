using Vint.Core.Battle.Mode;
using Vint.Core.Battle.Player.Score.Events.Visual;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.State;
using Vint.Core.Battle.Tank.Temperature;
using Vint.Core.Battle.Weapons.Components.Config;
using Vint.Core.Battle.Weapons.Damage.Calculator;
using Vint.Core.Battle.Weapons.Weapon.Hit;
using Vint.Core.Config;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Weapons.Handlers.Impl;

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

        bool isEnemy = BattleTank.IsEnemy(targetTank) && !BattleTank.IsSameTeam(targetTank) || (BattleTank.Round.Properties.GetValue(BattleProperty.BattleMode) == BattleMode.DM);
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
