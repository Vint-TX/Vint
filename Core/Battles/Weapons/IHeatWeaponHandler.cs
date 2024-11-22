namespace Vint.Core.Battles.Weapons;

public interface IHeatWeaponHandler : ITemperatureWeaponHandler {
    float HeatDamage { get; }
}
