namespace Vint.Core.Battles.Modules.Interfaces;

public interface ITemperatureModule {
    public void TemperatureChanged(float before, float current, float max);
}