using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class TemperatureBlockModule : PassiveBattleModule, IAlwaysActiveModule, IModuleWithoutEffect {
    public override string ConfigPath => "garage/module/upgrade/properties/tempblock";

    public override Effect GetEffect() => throw new NotSupportedException();

    public override bool ActivationCondition => !Enabled;

    public bool CanBeDeactivated { get; set; }

    TemperatureConfigComponent OriginalTemperatureConfigComponent { get; set; } = null!;
    bool Enabled { get; set; }

    float Decrement { get; set; }
    float Increment { get; set; }

    public override Task Activate() {
        if (!CanBeActivated) return Task.CompletedTask;

        Enabled = true;
        CanBeDeactivated = false;

        Tank.TemperatureConfig.AutoDecrementInMs += Decrement;
        Tank.TemperatureConfig.AutoIncrementInMs += Increment;
        return Task.CompletedTask;
    }

    public Task Deactivate() {
        if (!Enabled || !CanBeDeactivated) return Task.CompletedTask;

        Enabled = false;
        Tank.TemperatureConfig = OriginalTemperatureConfigComponent.Clone();
        return Task.CompletedTask;
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        OriginalTemperatureConfigComponent = Tank.TemperatureConfig.Clone();
        Decrement = Leveling.GetStat<ModuleTempblockDecrementPropertyComponent>(ConfigPath, Level);
        Increment = Leveling.GetStat<ModuleTempblockIncrementPropertyComponent>(ConfigPath, Level);
    }
}
