using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class EngineerModule : PassiveBattleModule, IAlwaysActiveModule, IModuleWithoutEffect {
    public override string ConfigPath => "garage/module/upgrade/properties/engineer";

    public override Effect GetEffect() => throw new NotSupportedException();

    public override bool ActivationCondition => !Enabled;

    public bool CanBeDeactivated { get; set; }
    bool Enabled { get; set; }
    float Multiplier { get; set; }

    public override Task Activate() {
        if (!CanBeActivated) return Task.CompletedTask;

        Enabled = true;
        CanBeDeactivated = false;

        Tank.SupplyDurationMultiplier *= Multiplier;
        return Task.CompletedTask;
    }

    public Task Deactivate() {
        if (!Enabled || !CanBeDeactivated) return Task.CompletedTask;

        Enabled = false;
        Tank.SupplyDurationMultiplier /= Multiplier;
        return Task.CompletedTask;
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Multiplier = Leveling.GetStat<ModuleEngineerEffectDurationFactorPropertyComponent>(ConfigPath, Level);
    }
}
