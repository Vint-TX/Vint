namespace Vint.Core.Battles.Weapons;

public interface IHeatWeaponHandler : ITemperatureWeaponHandler {
    public float HeatDamage { get; }
}