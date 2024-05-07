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

    public override void Activate() {
        if (!CanBeActivated) return;

        JumpImpactEffect? effect = Tank.Effects.OfType<JumpImpactEffect>().SingleOrDefault();

        if (effect != null) return;

        base.Activate();
        GetEffect().Activate();
    }

    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);

        Force = Leveling.GetStat<JumpImpactForceMultPropertyComponent>(ConfigPath, Level);
        WorkingTemperature = -Leveling.GetStat<JumpImpactWorkingTemperaturePropertyComponent>(ConfigPath, Level);
    }

    public override void TryUnblock() {
        if (Tank.Temperature < WorkingTemperature) return;

        base.TryUnblock();
    }

    public override void TryBlock(bool force = false, long blockTimeMs = 0) {
        if (!force && Tank.Temperature >= WorkingTemperature) return;

        base.TryBlock(force, blockTimeMs);
    }

    public void OnTemperatureChanged(float before, float current, float min, float max) {
        if (current >= WorkingTemperature) TryUnblock();
        else TryBlock(true);
    }
}
