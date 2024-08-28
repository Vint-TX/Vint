using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(596921121)]
public class TemperatureBlockModule : PassiveBattleModule, IAlwaysActiveModule, IModuleWithoutEffect {
    public override string ConfigPath => "garage/module/upgrade/properties/tempblock";

    public override Effect GetEffect() => throw new NotSupportedException();

    protected override bool ActivationCondition => !IsActive;

    public bool CanBeDeactivated { get; set; }

    TemperatureConfigComponent OriginalTemperatureConfigComponent { get; set; } = null!;
    public bool IsActive { get; private set; }

    float Decrement { get; set; }
    float Increment { get; set; }

    public override Task Activate() {
        if (!CanBeActivated) return Task.CompletedTask;

        IsActive = true;
        CanBeDeactivated = false;

        Tank.TemperatureConfig.AutoDecrementInMs += Decrement;
        Tank.TemperatureConfig.AutoIncrementInMs += Increment;
        return Task.CompletedTask;
    }

    public Task Deactivate() {
        if (!IsActive || !CanBeDeactivated) return Task.CompletedTask;

        IsActive = false;
        Tank.TemperatureConfig = OriginalTemperatureConfigComponent.Clone();
        return Task.CompletedTask;
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        OriginalTemperatureConfigComponent = Tank.TemperatureConfig.Clone();
        Decrement = GetStat<ModuleTempblockDecrementPropertyComponent>();
        Increment = GetStat<ModuleTempblockIncrementPropertyComponent>();
    }
}
