using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server.Damage;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public abstract class DiscreteTankWeaponHandler : TankWeaponHandler, IDiscreteWeaponHandler {
    protected DiscreteTankWeaponHandler(BattleTank battleTank) : base(battleTank) {
        if (ConfigManager.TryGetComponent(MarketConfigPath, out MinDamagePropertyComponent? minDamageComponent) &&
            ConfigManager.TryGetComponent(MarketConfigPath, out MaxDamagePropertyComponent? maxDamageComponent)) {
            MinDamage = minDamageComponent.FinalValue;
            MaxDamage = maxDamageComponent.FinalValue;
        } else {
            MinDamage = ConfigManager.GetComponent<AimingMinDamagePropertyComponent>(MarketConfigPath)
                .FinalValue;

            MaxDamage = ConfigManager.GetComponent<AimingMaxDamagePropertyComponent>(MarketConfigPath)
                .FinalValue;
        }

        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(MarketConfigPath)
            .CooldownIntervalSec);
    }

    public float MinDamage { get; }
    public float MaxDamage { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
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
}
