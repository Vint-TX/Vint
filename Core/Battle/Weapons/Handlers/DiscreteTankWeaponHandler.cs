using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Tank.State;
using Vint.Core.Battle.Weapons.Damage.Calculator;
using Vint.Core.Battle.Weapons.Damage.Components;
using Vint.Core.Battle.Weapons.Parameters.Components;
using Vint.Core.Battle.Weapons.Weapon.Hit;
using Vint.Core.Config;

namespace Vint.Core.Battle.Weapons.Handlers;

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
