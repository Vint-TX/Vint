using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class AdrenalineModule : PassiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/adrenaline";

    public override bool ActivationCondition => Effect == null;

    float HpToTrigger { get; set; }
    float CooldownSpeedCoeff { get; set; }
    float DamageMultiplier { get; set; }

    AdrenalineEffect? Effect { get; set; }

    public override AdrenalineEffect GetEffect() => new(Tank, Level, CooldownSpeedCoeff, DamageMultiplier);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        Effect = GetEffect();
        Effect.Deactivated += Deactivated;
        await Effect.Activate();
        await base.Activate();
    }

    public async Task OnHealthChanged(float before, float current, float max) {
        if (current > HpToTrigger || current == 0) await TryDeactivate();
        else await Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        HpToTrigger = Leveling.GetStat<ModuleAdrenalineEffectMaxHPPercentWorkingPropertyComponent>(ConfigPath, Level) * Tank.MaxHealth;
        CooldownSpeedCoeff = Leveling.GetStat<ModuleAdrenalineEffectCooldownSpeedCoeffPropertyComponent>(ConfigPath, Level);
        DamageMultiplier = Leveling.GetStat<ModuleDamageEffectMaxFactorPropertyComponent>(ConfigPath, Level);
    }

    Task TryDeactivate() =>
        Effect == null
            ? Task.CompletedTask
            : Effect.Deactivate();

    void Deactivated() => Effect = null;
}
