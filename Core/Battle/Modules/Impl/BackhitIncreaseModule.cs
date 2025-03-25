using Vint.Core.Battle.Effects.Components.Config.BackHit;
using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(-2075784110)]
public class BackhitIncreaseModule : PassiveBattleModule, IAlwaysActiveModule {
    public override string ConfigPath => "garage/module/upgrade/properties/backhitincrease";

    float Multiplier { get; set; }

    public override BackhitIncreaseEffect GetEffect() => new(Tank, Level, Multiplier);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        BackhitIncreaseEffect? effect = Tank
            .Effects
            .OfType<BackhitIncreaseEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await GetEffect()
            .Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Multiplier = GetStat<ModuleBackhitModificatorEffectPropertyComponent>();
    }
}
