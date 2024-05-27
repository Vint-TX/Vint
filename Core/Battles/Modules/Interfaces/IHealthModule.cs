namespace Vint.Core.Battles.Modules.Interfaces;

public interface IHealthModule {
    public Task OnHealthChanged(float before, float current, float max);
}
