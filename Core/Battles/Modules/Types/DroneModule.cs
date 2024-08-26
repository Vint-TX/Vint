using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(1392039140)]
public class DroneModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/drone";

    public override DroneEffect GetEffect() => new(Drones.Count(), MarketEntity, Duration, ActivationTime, TargetingDistance, Damage, Tank, Level);

    IEnumerable<DroneEffect> Drones => Tank.Effects.OfType<DroneEffect>();
    IEnumerable<DroneEffect> DronesSorted => Drones.OrderBy(drone => drone.Index);

    TimeSpan Duration { get; set; }
    TimeSpan ActivationTime { get; set; }
    int CountLimit { get; set; }
    float Damage { get; set; }
    float TargetingDistance { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        await GetEffect().Activate();

        while (Drones.Count() > CountLimit)
            await DronesSorted.First().Deactivate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Duration = TimeSpan.FromMilliseconds(GetStat<ModuleEffectDurationPropertyComponent>());
        ActivationTime = TimeSpan.FromMilliseconds(GetStat<ModuleEffectActivationTimePropertyComponent>());
        CountLimit = (int)GetStat<ModuleLimitBundleEffectCountPropertyComponent>();
        TargetingDistance = GetStat<ModuleEffectTargetingDistancePropertyComponent>();

        float minDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
        float maxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();

        Damage = (minDamage + maxDamage) / 2;
    }
}
