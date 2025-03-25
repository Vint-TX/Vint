namespace Vint.Core.Battle.Weapons;

public interface IHeatWeaponHandler : ITemperatureWeaponHandler {
    float HeatDamage { get; }
}
