using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(1458405023)]
public class SpiderMineModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/spidermine";

    public override SpiderMineEffect GetEffect() => new(Mines.Count(), MarketEntity, ActivationTime, TargetingDistance, BeginHideDistance, HideRange,
        Impact, MinSplashDamagePercent, RadiusOfMaxSplashDamage, RadiusOfMinSplashDamage, MaxDamage, MinDamage, Speed, Acceleration, Energy,
        IdleEnergyDrain, ChasingEnergyDrain, Tank, Level);

    IEnumerable<SpiderMineEffect> Mines => Tank.Effects.OfType<SpiderMineEffect>();
    IEnumerable<SpiderMineEffect> MinesSorted => Mines.OrderBy(mine => mine.Index);

    TimeSpan ActivationTime { get; set; }
    int CountLimit { get; set; }
    float MinDamage { get; set; }
    float MaxDamage { get; set; }
    float Impact { get; set; }
    float HideRange { get; set; }
    float BeginHideDistance { get; set; }
    float RadiusOfMaxSplashDamage { get; set; }
    float RadiusOfMinSplashDamage { get; set; }
    float MinSplashDamagePercent { get; set; }
    float Acceleration { get; set; }
    float Speed { get; set; }
    float Energy { get; set; }
    float IdleEnergyDrain { get; set; }
    float ChasingEnergyDrain { get; set; }
    float TargetingDistance { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        await GetEffect().Activate();

        while (Mines.Count() > CountLimit)
            await MinesSorted.First().ForceDeactivate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        ActivationTime = TimeSpan.FromMilliseconds(GetStat<ModuleEffectActivationTimePropertyComponent>());
        CountLimit = (int)GetStat<ModuleLimitBundleEffectCountPropertyComponent>();
        MinDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
        MaxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();
        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
        HideRange = GetStat<ModuleMineEffectHideRangePropertyComponent>();
        BeginHideDistance = GetStat<ModuleMineEffectBeginHideDistancePropertyComponent>();
        RadiusOfMaxSplashDamage = GetStat<ModuleMineEffectSplashDamageMaxRadiusPropertyComponent>();
        RadiusOfMinSplashDamage = GetStat<ModuleMineEffectSplashDamageMinRadiusPropertyComponent>();
        MinSplashDamagePercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>();
        Acceleration = GetStat<ModuleEffectAccelerationPropertyComponent>();
        Speed = GetStat<ModuleEffectSpeedPropertyComponent>();
        Energy = GetStat<ModuleSpiderMineEffectEnergyPropertyComponent>();
        IdleEnergyDrain = GetStat<ModuleSpiderMineEffectIdleEnergyDrainRatePropertyComponent>();
        ChasingEnergyDrain = GetStat<ModuleSpiderMineEffectChasingEnergyDrainRatePropertyComponent>();
        TargetingDistance = GetStat<ModuleEffectTargetingDistancePropertyComponent>();
    }
}
