namespace Vint.Core.Battles.Modules.Interfaces;

public interface ITemperatureModule {
    public void OnTemperatureChanged(float before, float current, float min, float max);
}
