using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server;

namespace Vint.Core.Battles.Weapons;

public class SmokyWeaponHandler : DiscreteTankWeaponHandler {
    public SmokyWeaponHandler(BattleTank battleTank) : base(battleTank) {
        CriticalDamage = ConfigManager.GetComponent<CriticalDamagePropertyComponent>(MarketConfigPath).FinalValue;
        StartCriticalProbability = ConfigManager.GetComponent<StartCriticalProbabilityPropertyComponent>(MarketConfigPath).FinalValue;
        CriticalProbabilityDelta = ConfigManager.GetComponent<CriticalProbabilityDeltaPropertyComponent>(MarketConfigPath).FinalValue;
        AfterCriticalProbability = ConfigManager.GetComponent<AfterCriticalHitProbabilityPropertyComponent>(MarketConfigPath).FinalValue;
        MaxCriticalProbability = ConfigManager.GetComponent<MaxCriticalProbabilityPropertyComponent>(MarketConfigPath).FinalValue;

        StartDamageProgression = ConfigManager.GetComponent<StartDamageProgressionPropertyComponent>(MarketConfigPath).FinalValue;
        MaxDamageProgression = ConfigManager.GetComponent<MaxDamageProgressionPropertyComponent>(MarketConfigPath).FinalValue;
        DamageProgressionDelta = ConfigManager.GetComponent<DamageProgressionDeltaPropertyComponent>(MarketConfigPath).FinalValue;
        DamageProgressionReset =
            TimeSpan.FromSeconds(ConfigManager.GetComponent<DamageProgressionResetTimeSecPropertyComponent>(MarketConfigPath).FinalValue);

        DamageProgressionMiddle = (StartDamageProgression + MaxDamageProgression) / 2;
    }

    Random Random { get; } = new();

    float CurrentCriticalProbability { get; set; }
    float CriticalDamage { get; }
    float StartCriticalProbability { get; }
    float CriticalProbabilityDelta { get; }
    float AfterCriticalProbability { get; }
    float MaxCriticalProbability { get; }

    float DamageProgressionMiddle { get; }
    float StartDamageProgression { get; }
    float MaxDamageProgression { get; }
    float DamageProgressionDelta { get; }
    TimeSpan DamageProgressionReset { get; }

    DateTimeOffset LastHit { get; set; }
    long LastIncarnationId { get; set; }
    float CurrentDamage { get; set; }

    bool ShouldResetDamage { get; set; }
    int PrevShotId { get; set; }
    int PrevHitId { get; set; }

    public override int MaxHitTargets => 1;

    public float GetProgressedDamage(long incarnationId, out bool isBig) {
        if (ShouldResetDamage ||
            DateTimeOffset.UtcNow - LastHit > DamageProgressionReset ||
            incarnationId != LastIncarnationId)
            CurrentDamage = StartDamageProgression;
        else
            CurrentDamage += DamageProgressionDelta;

        CurrentDamage = Math.Min(CurrentDamage, MaxDamageProgression);
        LastIncarnationId = incarnationId;
        LastHit = DateTimeOffset.UtcNow;
        ShouldResetDamage = false;

        isBig = CurrentDamage >= DamageProgressionMiddle;
        return CurrentDamage;
    }

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

    public void OnShot(int id) {
        if (PrevShotId != PrevHitId)
            ShouldResetDamage = true;

        PrevShotId = id;
    }

    public void OnHit(int id, bool isStatic) {
        if (isStatic)
            ShouldResetDamage = true;

        PrevHitId = id;
    }

    public override void OnTankEnable() {
        base.OnTankEnable();
        CurrentDamage = StartDamageProgression;
        CurrentCriticalProbability = StartCriticalProbability;
    }
}
