using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Battles.Weapons;

public abstract class DiscreteWeaponHandler : WeaponHandler {
    protected DiscreteWeaponHandler(BattleTank battleTank) : base(battleTank) {
        if (ConfigManager.TryGetComponent(MarketConfigPath, out Damage.MinDamagePropertyComponent? minDamageComponent) &&
            ConfigManager.TryGetComponent(MarketConfigPath, out Damage.MaxDamagePropertyComponent? maxDamageComponent)) {
            MinDamage = minDamageComponent.FinalValue;
            MaxDamage = maxDamageComponent.FinalValue;
        } else {
            MinDamage = ConfigManager.GetComponent<Damage.AimingMinDamagePropertyComponent>(MarketConfigPath).FinalValue;
            MaxDamage = ConfigManager.GetComponent<Damage.AimingMaxDamagePropertyComponent>(MarketConfigPath).FinalValue;
        }

        Cooldown = ConfigManager.GetComponent<WeaponCooldownComponent>(MarketConfigPath).CooldownIntervalSec;
    }

    public float MinDamage { get; }
    public float MaxDamage { get; }
}