using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class FireRingModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/firering";

    public override FireRingEffect GetEffect() => new(Cooldown, MarketEntity, Radius, MinDamagePercent, Impact, TemperatureLimit, TemperatureDelta,
        HeatDamage, Tank, Level);

    float Radius { get; set; }
    float MinDamagePercent { get; set; }
    float Impact { get; set; }
    float HeatDamage { get; set; }
    float TemperatureLimit { get; set; }
    float TemperatureDelta { get; set; }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Radius = Leveling.GetStat<ModuleEffectSplashRadiusPropertyComponent>(ConfigPath, Level);
        HeatDamage = Leveling.GetStat<ModuleFireRingEffectHeatDamagePropertyComponent>(ConfigPath, Level);
        TemperatureLimit = Leveling.GetStat<ModuleFireRingEffectTemperatureLimitPropertyComponent>(ConfigPath, Level);
        TemperatureDelta = Leveling.GetStat<ModuleFireRingEffectTemperatureDeltaPropertyComponent>(ConfigPath, Level);
        MinDamagePercent = Leveling.GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>(ConfigPath, Level) * 100;
        Impact = Leveling.GetStat<ModuleEffectImpactPropertyComponent>(ConfigPath, Level);
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        FireRingEffect? effect = Tank.Effects.OfType<FireRingEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }
}
