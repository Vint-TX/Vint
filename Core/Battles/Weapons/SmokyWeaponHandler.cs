using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Battles.Weapons;

public class SmokyWeaponHandler : DiscreteWeaponHandler {
    public SmokyWeaponHandler(BattleTank battleTank) : base(battleTank) {
        CriticalDamage = ConfigManager.GetComponent<CriticalDamagePropertyComponent>(MarketConfigPath).FinalValue;
        StartCriticalProbability = ConfigManager.GetComponent<StartCriticalProbabilityPropertyComponent>(MarketConfigPath).FinalValue;
        CriticalProbabilityDelta = ConfigManager.GetComponent<CriticalProbabilityDeltaPropertyComponent>(MarketConfigPath).FinalValue;
        AfterCriticalProbability = ConfigManager.GetComponent<AfterCriticalHitProbabilityPropertyComponent>(MarketConfigPath).FinalValue;
        MaxCriticalProbability = ConfigManager.GetComponent<MaxCriticalProbabilityPropertyComponent>(MarketConfigPath).FinalValue;
    }

    Random Random { get; } = new();

    float CurrentCriticalProbability { get; set; }
    float CriticalDamage { get; }
    float StartCriticalProbability { get; }
    float CriticalProbabilityDelta { get; }
    float AfterCriticalProbability { get; }
    float MaxCriticalProbability { get; }

    public override int MaxHitTargets => 1;

    public bool TryCalculateCriticalDamage(ref float damage) {
        if (Random.NextSingle() < CurrentCriticalProbability) {
            damage += CriticalDamage;
            CurrentCriticalProbability = AfterCriticalProbability;
            return true;
        }

        CurrentCriticalProbability =
            Math.Clamp(CurrentCriticalProbability + CriticalProbabilityDelta, AfterCriticalProbability, MaxCriticalProbability);

        return false;
    }

    public override void OnTankEnable() {
        base.OnTankEnable();
        CurrentCriticalProbability = StartCriticalProbability;
    }
}