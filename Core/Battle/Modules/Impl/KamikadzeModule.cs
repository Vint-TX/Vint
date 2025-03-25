using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-1603423529)]
public class KamikadzeModule : TriggerBattleModule, IDeathModule {
    public override string ConfigPath => "garage/module/upgrade/properties/kamikadze";

    float Impact { get; set; }
    float Radius { get; set; }
    float MinPercent { get; set; }
    float MinDamage { get; set; }
    float MaxDamage { get; set; }

    public Task OnDeath() => Activate();

    public override KamikadzeEffect GetEffect() => new(Cooldown, MarketEntity, Radius, MinPercent, MaxDamage, MinDamage, Impact, Tank, Level);

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
        Radius = GetStat<ModuleEffectSplashRadiusPropertyComponent>();
        MinPercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>() * 100;
        MinDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
        MaxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        KamikadzeEffect? effect = Tank.Effects
            .OfType<KamikadzeEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }
}
