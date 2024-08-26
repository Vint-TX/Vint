using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-1027949361)]
public class EngineerModule : PassiveBattleModule, IAlwaysActiveModule, IModuleWithoutEffect, INeutralizeEMPModule {
    public override string ConfigPath => "garage/module/upgrade/properties/engineer";

    public override Effect GetEffect() => throw new NotSupportedException();

    protected override bool ActivationCondition => !IsActive;

    public bool CanBeDeactivated { get; set; }
    public bool IsActive { get; private set; }
    float Multiplier { get; set; }

    public override Task Activate() {
        if (!CanBeActivated) return Task.CompletedTask;

        IsActive = true;
        CanBeDeactivated = false;

        Tank.SupplyDurationMultiplier *= Multiplier;
        return Task.CompletedTask;
    }

    public Task Deactivate() {
        if (!IsActive || !CanBeDeactivated) return Task.CompletedTask;

        IsActive = false;
        Tank.SupplyDurationMultiplier /= Multiplier;
        return Task.CompletedTask;
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Multiplier = Leveling.GetStat<ModuleEngineerEffectDurationFactorPropertyComponent>(ConfigPath, Level);
    }
}
