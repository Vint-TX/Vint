using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Components.Server.Modules.Effect.EMP;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-1493372159)]
public class EMPModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/emp";

    public override EMPEffect GetEffect() => new(Tank, Level, Duration, Radius);

    TimeSpan Duration { get; set; }
    float Radius { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        EMPEffect? effect = Tank.Effects.OfType<EMPEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Duration = TimeSpan.FromMilliseconds(GetStat<ModuleEffectDurationPropertyComponent>());
        Radius = GetStat<ModuleEMPEffectRadiusPropertyComponent>();
    }
}
