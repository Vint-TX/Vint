using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Battles.Weapons;

public abstract class StreamWeaponHandler : WeaponHandler {
    protected StreamWeaponHandler(BattleTank battleTank) : base(battleTank) {
        Cooldown = ConfigManager.GetComponent<WeaponCooldownComponent>(BattleConfigPath).CooldownIntervalSec;
        DamagePerSecond = ConfigManager.GetComponent<DamagePerSecondPropertyComponent>(MarketConfigPath).FinalValue;
    }

    public float DamagePerSecond { get; }
}