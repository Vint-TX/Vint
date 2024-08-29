using Vint.Core.Battles.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Weapon;

namespace Vint.Core.Battles.Weapons;

public class FreezeWeaponHandler : StreamWeaponHandler {
    public FreezeWeaponHandler(BattleTank battleTank) : base(battleTank) {
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta =
            ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * (float)Cooldown.TotalSeconds;
    }

    public override int MaxHitTargets => int.MaxValue;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
}
