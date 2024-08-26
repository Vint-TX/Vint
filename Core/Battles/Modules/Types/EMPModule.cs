using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

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

        Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(ConfigPath, Level));
        Radius = Leveling.GetStat<ModuleEMPEffectRadiusPropertyComponent>(ConfigPath, Level);
    }
}
