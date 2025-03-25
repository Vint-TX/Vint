using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.TempBlock;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(596921121)]
public class TemperatureBlockModule : PassiveBattleModule, IAlwaysActiveModule, IModuleWithoutEffect {
    public override string ConfigPath => "garage/module/upgrade/properties/tempblock";

    protected override bool ActivationCondition => !IsActive;

    float Decrement { get; set; }
    float Increment { get; set; }

    public bool CanBeDeactivated { get; set; }

    public bool IsActive { get; private set; }

    public Task Deactivate() {
        if (!IsActive || !CanBeDeactivated) return Task.CompletedTask;

        IsActive = false;
        Tank.TemperatureProcessor.ChangeTemperatureConfig(-Increment, -Decrement);
        return Task.CompletedTask;
    }

    public override Effect GetEffect() => throw new NotSupportedException();

    public override Task Activate() {
        if (!CanBeActivated) return Task.CompletedTask;

        IsActive = true;
        CanBeDeactivated = false;

        Tank.TemperatureProcessor.ChangeTemperatureConfig(Increment, Decrement);
        return Task.CompletedTask;
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Decrement = GetStat<ModuleTempblockDecrementPropertyComponent>();
        Increment = GetStat<ModuleTempblockIncrementPropertyComponent>();
    }
}
