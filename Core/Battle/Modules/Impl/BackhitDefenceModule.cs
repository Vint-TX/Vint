using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.BackHit;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-1962420821)]
public class BackhitDefenceModule : PassiveBattleModule, IAlwaysActiveModule {
    public override string ConfigPath => "garage/module/upgrade/properties/backhitdefence";

    float Multiplier { get; set; }

    public override BackhitDefenceEffect GetEffect() => new(Tank, Level, Multiplier);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        BackhitDefenceEffect? effect = Tank
            .Effects
            .OfType<BackhitDefenceEffect>()
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
