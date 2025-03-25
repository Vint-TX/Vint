using Vint.Core.Battle.Effects.Components.Config.Adrenaline;
using Vint.Core.Battle.Effects.Components.Config.IncreasedDamage;
using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(1367280061)]
public class AdrenalineModule : PassiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/adrenaline";

    protected override bool ActivationCondition => Effect == null;

    float HpToTrigger { get; set; }
    float CooldownSpeedCoeff { get; set; }
    float DamageMultiplier { get; set; }

    AdrenalineEffect? Effect { get; set; }

    public async Task OnHealthChanged(float before, float current, float max) {
        if (current > HpToTrigger ||
            current == 0) await TryDeactivate();
        else await Activate();
    }

    public override AdrenalineEffect GetEffect() => new(Tank, Level, CooldownSpeedCoeff, DamageMultiplier);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        Effect = GetEffect();
        Effect.Deactivated += Deactivated;
        await Effect.Activate();
        await base.Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        HpToTrigger = GetStat<ModuleAdrenalineEffectMaxHPPercentWorkingPropertyComponent>() * Tank.MaxHealth;
        CooldownSpeedCoeff = GetStat<ModuleAdrenalineEffectCooldownSpeedCoeffPropertyComponent>();
        DamageMultiplier = GetStat<ModuleDamageEffectMaxFactorPropertyComponent>();
    }

    Task TryDeactivate() =>
        Effect == null
            ? Task.CompletedTask
            : Effect.Deactivate();

    void Deactivated() => Effect = null;
}
