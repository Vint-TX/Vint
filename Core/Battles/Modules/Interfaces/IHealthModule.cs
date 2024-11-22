namespace Vint.Core.Battles.Modules.Interfaces;

public interface IHealthModule {
    Task OnHealthChanged(float before, float current, float max);
}
