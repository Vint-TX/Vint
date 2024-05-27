using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class KamikadzeModule : TriggerBattleModule, IDeathModule {
    public override string ConfigPath => "garage/module/upgrade/properties/kamikadze";

    public override KamikadzeEffect GetEffect() => new(Cooldown, MarketEntity, Radius, MinPercent, MaxDamage, MinDamage, Impact, Tank, Level);

    float Impact { get; set; }
    float Radius { get; set; }
    float MinPercent { get; set; }
    float MinDamage { get; set; }
    float MaxDamage { get; set; }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Impact = Leveling.GetStat<ModuleEffectImpactPropertyComponent>(ConfigPath, Level);
        Radius = Leveling.GetStat<ModuleEffectSplashRadiusPropertyComponent>(ConfigPath, Level);
        MinPercent = Leveling.GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>(ConfigPath, Level) * 100;
        MinDamage = Leveling.GetStat<ModuleEffectMinDamagePropertyComponent>(ConfigPath, Level);
        MaxDamage = Leveling.GetStat<ModuleEffectMaxDamagePropertyComponent>(ConfigPath, Level);
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        KamikadzeEffect? effect = Tank.Effects.OfType<KamikadzeEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }

    public Task OnDeath() => Activate();
}
