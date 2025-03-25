using Vint.Core.Battle.Effects.Components.Config.Common;
using Vint.Core.Battle.Effects.Components.Config.FireRing;
using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(1896579342)]
public class FireRingModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/firering";

    float Radius { get; set; }
    float MinDamagePercent { get; set; }
    float Impact { get; set; }
    float HeatDamage { get; set; }
    float TemperatureLimit { get; set; }
    float TemperatureDelta { get; set; }

    public override FireRingEffect GetEffect() => new(Cooldown,
        MarketEntity,
        Radius,
        MinDamagePercent,
        Impact,
        TemperatureLimit,
        TemperatureDelta,
        HeatDamage,
        Tank,
        Level);

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Radius = GetStat<ModuleEffectSplashRadiusPropertyComponent>();
        HeatDamage = GetStat<ModuleFireRingEffectHeatDamagePropertyComponent>();
        TemperatureLimit = GetStat<ModuleEffectTemperatureLimitPropertyComponent>();
        TemperatureDelta = GetStat<ModuleEffectTemperatureDeltaPropertyComponent>();
        MinDamagePercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>() * 100;
        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        FireRingEffect? effect = Tank
            .Effects
            .OfType<FireRingEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }
}
