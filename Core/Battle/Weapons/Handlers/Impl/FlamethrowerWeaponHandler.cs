using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Weapon;

namespace Vint.Core.Battle.Weapons;

public class FlamethrowerWeaponHandler : StreamWeaponHandler, IHeatWeaponHandler {
    public FlamethrowerWeaponHandler(BattleTank battleTank) : base(battleTank) {
        HeatDamage = ConfigManager.GetComponent<HeatDamagePropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta = (float)(ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * Cooldown.TotalSeconds);
    }

    public override int MaxHitTargets => int.MaxValue;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
    public float HeatDamage { get; }
}
