using Vint.Core.Battle.Effects.Components.Config.AcceleratedGears;
using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(1365914179)]
public class AcceleratedGearsModule : PassiveBattleModule, IAlwaysActiveModule {
    public override string ConfigPath => "garage/module/upgrade/properties/acceleratedgears";

    float TurretSpeed { get; set; }
    float TurretAcceleration { get; set; }
    float HullRotation { get; set; }

    public override AcceleratedGearsEffect GetEffect() => new(Tank, Level, TurretSpeed, TurretAcceleration, HullRotation);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        AcceleratedGearsEffect? effect = Tank
            .Effects
            .OfType<AcceleratedGearsEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await GetEffect()
            .Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        TurretSpeed = GetStat<ModuleAcceleratedGearsEffectTurretSpeedPropertyComponent>();
        TurretAcceleration = GetStat<ModuleAcceleratedGearsEffectTurretAccelerationPropertyComponent>();
        HullRotation = GetStat<ModuleAcceleratedGearsEffectHullRotationSpeedPropertyComponent>();
    }
}
