namespace Vint.Core.Battle.Weapons.Handlers;

public interface IHeatWeaponHandler : ITemperatureWeaponHandler {
    float HeatDamage { get; }
}
