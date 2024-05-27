using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class JumpImpactModule : ActiveBattleModule, ITemperatureModule {
    public override string ConfigPath => "garage/module/upgrade/properties/jumpimpact";

    public override JumpImpactEffect GetEffect() => new(Tank, Level, Force);

    public override bool ActivationCondition => Tank.Temperature >= WorkingTemperature;

    float Force { get; set; }
    float WorkingTemperature { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        JumpImpactEffect? effect = Tank.Effects.OfType<JumpImpactEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Force = Leveling.GetStat<JumpImpactForceMultPropertyComponent>(ConfigPath, Level);
        WorkingTemperature = -Leveling.GetStat<JumpImpactWorkingTemperaturePropertyComponent>(ConfigPath, Level);
    }

    public override async Task TryUnblock() {
        if (Tank.Temperature < WorkingTemperature) return;

        await base.TryUnblock();
    }

    public override async Task TryBlock(bool force = false, long blockTimeMs = 0) {
        if (!force && Tank.Temperature >= WorkingTemperature) return;

        await base.TryBlock(force, blockTimeMs);
    }

    public async Task OnTemperatureChanged(float before, float current, float min, float max) {
        if (current >= WorkingTemperature) await TryUnblock();
        else await TryBlock(true);
    }
}
