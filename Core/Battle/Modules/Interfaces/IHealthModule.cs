namespace Vint.Core.Battle.Modules.Interfaces;

public interface IHealthModule {
    Task OnHealthChanged(float before, float current, float max);
}
