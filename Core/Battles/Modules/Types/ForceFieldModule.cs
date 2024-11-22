using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-1597839790)]
public class ForceFieldModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/forcefield";

    TimeSpan Duration { get; set; }

    public override ForceFieldEffect GetEffect() => new(Duration, Tank, Level);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        ForceFieldEffect? effect = Tank
            .Effects
            .OfType<ForceFieldEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Duration = TimeSpan.FromMilliseconds(GetStat<ModuleEffectDurationPropertyComponent>());
    }
}
