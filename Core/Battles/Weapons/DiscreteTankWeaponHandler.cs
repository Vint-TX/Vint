using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public abstract class DiscreteTankWeaponHandler : TankWeaponHandler, IDiscreteWeaponHandler {
    protected DiscreteTankWeaponHandler(BattleTank battleTank) : base(battleTank) {
        if (ConfigManager.TryGetComponent(MarketConfigPath, out MinDamagePropertyComponent? minDamageComponent) &&
            ConfigManager.TryGetComponent(MarketConfigPath, out MaxDamagePropertyComponent? maxDamageComponent)) {
            MinDamage = minDamageComponent.FinalValue;
            MaxDamage = maxDamageComponent.FinalValue;
        } else {
            MinDamage = ConfigManager.GetComponent<AimingMinDamagePropertyComponent>(MarketConfigPath).FinalValue;
            MaxDamage = ConfigManager.GetComponent<AimingMaxDamagePropertyComponent>(MarketConfigPath).FinalValue;
        }

        Cooldown = TimeSpan.FromSeconds(ConfigManager.GetComponent<WeaponCooldownComponent>(MarketConfigPath).CooldownIntervalSec);
    }

    public float MinDamage { get; }
    public float MaxDamage { get; }

    public override async Task Fire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }
}
