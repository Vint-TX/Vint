using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Components.Battle.Effect.Type.Mine;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Components.Server.Modules.Effect.Mine;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(1133911248)]
public class MineModule : ActiveBattleModule {
    const float MineHalfSize = 0.5f;

    public override string ConfigPath => "garage/module/upgrade/properties/mine";

    public override MineEffect GetEffect() => new(GenerateIndex(), MarketEntity, ActivationTime, ExplosionDelay, BeginHideDistance, HideRange,
        TriggeringArea, Impact, MinSplashDamagePercent, RadiusOfMaxSplashDamage, RadiusOfMinSplashDamage, MaxDamage, MinDamage, Tank, Level);

    IEnumerable<MineEffect> Mines => Tank.Effects.OfType<MineEffect>();
    IEnumerable<MineEffect> MinesSorted => Mines.OrderBy(mine => mine.Index);

    TimeSpan ActivationTime { get; set; }
    TimeSpan ExplosionDelay { get; set; }
    int CountLimit { get; set; }
    float MinDamage { get; set; }
    float MaxDamage { get; set; }
    float Impact { get; set; }
    float HideRange { get; set; }
    float BeginHideDistance { get; set; }
    float RadiusOfMaxSplashDamage { get; set; }
    float RadiusOfMinSplashDamage { get; set; }
    float MinSplashDamagePercent { get; set; }
    float TriggeringArea { get; set; }

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
        ExplosionDelay = TimeSpan.FromMilliseconds(GetStat<ModuleMineEffectExplosionDelayMSPropertyComponent>());
        CountLimit = (int)GetStat<ModuleLimitBundleEffectCountPropertyComponent>();
        MinDamage = GetStat<ModuleEffectMinDamagePropertyComponent>();
        MaxDamage = GetStat<ModuleEffectMaxDamagePropertyComponent>();
        Impact = GetStat<ModuleEffectImpactPropertyComponent>();
        HideRange = GetStat<ModuleMineEffectHideRangePropertyComponent>();
        BeginHideDistance = GetStat<ModuleMineEffectBeginHideDistancePropertyComponent>();
        RadiusOfMaxSplashDamage = GetStat<ModuleMineEffectSplashDamageMaxRadiusPropertyComponent>();
        RadiusOfMinSplashDamage = GetStat<ModuleMineEffectSplashDamageMinRadiusPropertyComponent>();
        MinSplashDamagePercent = GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>();
        TriggeringArea = GetStat<ModuleMineEffectTriggeringAreaPropertyComponent>() + MineHalfSize;
    }

    protected override async Task<IEntity> CreateBattleModule() {
        IEntity entity = await base.CreateBattleModule();

        await entity.AddComponent<StaticMineModuleComponent>();
        return entity;
    }

    int GenerateIndex() => Battle.MineProcessor.GenerateIndex();
}
