namespace Vint.Core.Battles.Modules.Interfaces;

public interface ITemperatureModule {
    public Task OnTemperatureChanged(float before, float current, float min, float max);
}
