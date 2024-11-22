using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Components.Server.Modules.Effect.FireRing;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

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
