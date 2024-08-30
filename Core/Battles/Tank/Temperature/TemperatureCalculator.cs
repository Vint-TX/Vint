using System.Diagnostics.Contracts;
using Vint.Core.Battles.Weapons;

namespace Vint.Core.Battles.Tank.Temperature;

public class TemperatureCalculator {
    [Pure]
    public static TemperatureAssist Calculate(BattleTank source, ITemperatureWeaponHandler weaponHandler, bool normalizeOnly) {
        TemperatureAssist assist = weaponHandler is IHeatWeaponHandler heatWeaponHandler
            ? new HeatTemperatureAssist(source, weaponHandler.TemperatureDuration, weaponHandler.TemperatureDelta, weaponHandler.TemperatureLimit,
                normalizeOnly, weaponHandler.MarketEntity, weaponHandler.BattleEntity, heatWeaponHandler.HeatDamage)
            : new TemperatureAssist(source, weaponHandler.TemperatureDuration, weaponHandler.TemperatureDelta, weaponHandler.TemperatureLimit,
                normalizeOnly, weaponHandler.MarketEntity, weaponHandler.BattleEntity);

        return assist;
    }
}
