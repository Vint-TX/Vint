using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class AcceleratedGearsModule : PassiveBattleModule, IAlwaysActiveModule {
    public override string ConfigPath => "garage/module/upgrade/properties/acceleratedgears";

    public override AcceleratedGearsEffect GetEffect() => new(Tank, Level, TurretSpeed, TurretAcceleration, HullRotation);

    float TurretSpeed { get; set; }
    float TurretAcceleration { get; set; }
    float HullRotation { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        AcceleratedGearsEffect? effect = Tank.Effects.OfType<AcceleratedGearsEffect>().SingleOrDefault();

        if (effect != null) return;

        await GetEffect().Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        TurretSpeed = Leveling.GetStat<ModuleAcceleratedGearsEffectTurretSpeedPropertyComponent>(ConfigPath, Level);
        TurretAcceleration = Leveling.GetStat<ModuleAcceleratedGearsEffectTurretAccelerationPropertyComponent>(ConfigPath, Level);
        HullRotation = Leveling.GetStat<ModuleAcceleratedGearsEffectHullRotationSpeedPropertyComponent>(ConfigPath, Level);
    }
}
