using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-1334156852)]
public class ExternalImpactModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/externalimpact";

    float Impact { get; set; }
    float Radius { get; set; }
    float MinPercent { get; set; }
    float MaxDamage { get; set; }
    float MinDamage { get; set; }

    public override ExternalImpactEffect GetEffect() => new(Cooldown, MarketEntity, Radius, MinPercent, MaxDamage, MinDamage, Impact, Tank, Level);

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
        Radius = GetStat<ModuleEffectSplashRadiusPropertyComponent>();
        MinPercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>() * 100;
        MaxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();
        MinDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        ExternalImpactEffect? effect = Tank
            .Effects
            .OfType<ExternalImpactEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();

        await GetEffect()
            .Activate();
    }
}
