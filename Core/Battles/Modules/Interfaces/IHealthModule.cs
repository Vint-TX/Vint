namespace Vint.Core.Battles.Modules.Interfaces;

public interface IHealthModule {
    public void OnHealthChanged(float before, float current, float max);
}