namespace Vint.Core.Battle.Modules.Interfaces;

public interface ITemperatureModule {
    Task OnTemperatureChanged(float before, float current, float min, float max);
}
