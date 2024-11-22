using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.JumpImpact;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(1327463523)]
public class JumpImpactModule : ActiveBattleModule, ITemperatureModule {
    public override string ConfigPath => "garage/module/upgrade/properties/jumpimpact";

    protected override bool ActivationCondition => Tank.TemperatureProcessor.Temperature >= WorkingTemperature;

    float Force { get; set; }
    float WorkingTemperature { get; set; }

    public async Task OnTemperatureChanged(float before, float current, float min, float max) {
        if (current >= WorkingTemperature) await TryUnblock();
        else await TryBlock();
    }

    public override JumpImpactEffect GetEffect() => new(Tank, Level, Force);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        JumpImpactEffect? effect = Tank
            .Effects
            .OfType<JumpImpactEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Force = GetStat<JumpImpactForceMultPropertyComponent>();
        WorkingTemperature = -GetStat<JumpImpactWorkingTemperaturePropertyComponent>();
    }

    public override async Task TryUnblock() {
        if (Tank.TemperatureProcessor.Temperature < WorkingTemperature) return;

        await base.TryUnblock();
    }
}
