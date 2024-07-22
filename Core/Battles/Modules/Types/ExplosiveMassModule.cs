using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class ExplosiveMassModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/explosivemass";

    public override ExplosiveMassEffect GetEffect() => new(Cooldown, MarketEntity, Radius, ActivationTime, MaxDamage, MinDamage, Tank, Level);

    float Radius { get; set; }
    float MaxDamage { get; set; }
    float MinDamage { get; set; }
    float ActivationTime { get; set; }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Radius = Leveling.GetStat<ModuleEffectTargetingDistancePropertyComponent>(ConfigPath, Level);
        MaxDamage = Leveling.GetStat<ModuleEffectMaxDamagePropertyComponent>(ConfigPath, Level);
        MinDamage = Leveling.GetStat<ModuleEffectMinDamagePropertyComponent>(ConfigPath, Level);
        ActivationTime = Leveling.GetStat<ModuleEffectActivationTimePropertyComponent>(ConfigPath, Level);
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        ExplosiveMassEffect? effect = Tank.Effects.OfType<ExplosiveMassEffect>().SingleOrDefault();

        if (effect != null) return;

        await base.Activate();
        await GetEffect().Activate();
    }
}
