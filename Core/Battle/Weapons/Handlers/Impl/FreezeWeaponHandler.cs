using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons.Components.Config;
using Vint.Core.Config;

namespace Vint.Core.Battle.Weapons.Handlers.Impl;

public class FreezeWeaponHandler : StreamWeaponHandler {
    public FreezeWeaponHandler(BattleTank battleTank) : base(battleTank) {
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta = (float)(ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * Cooldown.TotalSeconds);
    }

    public override int MaxHitTargets => int.MaxValue;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
}
