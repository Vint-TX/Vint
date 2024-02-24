using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Battles.Weapons;

public class FlamethrowerWeaponHandler : StreamWeaponHandler, IHeatWeaponHandler {
    public FlamethrowerWeaponHandler(BattleTank battleTank) : base(battleTank) {
        HeatDamage = ConfigManager.GetComponent<HeatDamagePropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureLimit = ConfigManager.GetComponent<TemperatureLimitPropertyComponent>(MarketConfigPath).FinalValue;
        TemperatureDelta =
            ConfigManager.GetComponent<DeltaTemperaturePerSecondPropertyComponent>(MarketConfigPath).FinalValue * (float)Cooldown.TotalSeconds;
    }

    public override int MaxHitTargets => int.MaxValue;
    public override float TemperatureLimit { get; }
    public override float TemperatureDelta { get; }
    public float HeatDamage { get; }
}