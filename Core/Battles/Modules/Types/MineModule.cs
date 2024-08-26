using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Effect.Type.Mine;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

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

        ActivationTime = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectActivationTimePropertyComponent>(ConfigPath, Level));
        ExplosionDelay = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleMineEffectExplosionDelayMSPropertyComponent>(ConfigPath, Level));
        CountLimit = (int)Leveling.GetStat<ModuleLimitBundleEffectCountPropertyComponent>(ConfigPath, Level);
        MinDamage = Leveling.GetStat<ModuleEffectMinDamagePropertyComponent>(ConfigPath, Level);
        MaxDamage = Leveling.GetStat<ModuleEffectMaxDamagePropertyComponent>(ConfigPath, Level);
        Impact = Leveling.GetStat<ModuleEffectImpactPropertyComponent>(ConfigPath, Level);
        HideRange = Leveling.GetStat<ModuleMineEffectHideRangePropertyComponent>(ConfigPath, Level);
        BeginHideDistance = Leveling.GetStat<ModuleMineEffectBeginHideDistancePropertyComponent>(ConfigPath, Level);
        RadiusOfMaxSplashDamage = Leveling.GetStat<ModuleMineEffectSplashDamageMaxRadiusPropertyComponent>(ConfigPath, Level);
        RadiusOfMinSplashDamage = Leveling.GetStat<ModuleMineEffectSplashDamageMinRadiusPropertyComponent>(ConfigPath, Level);
        MinSplashDamagePercent = Leveling.GetStat<ModuleEffectSplashDamageMinPercentPropertyComponent>(ConfigPath, Level);
        TriggeringArea = Leveling.GetStat<ModuleMineEffectTriggeringAreaPropertyComponent>(ConfigPath, Level) + MineHalfSize;
    }

    protected override async Task<IEntity> CreateBattleModule() {
        IEntity entity = await base.CreateBattleModule();

        await entity.AddComponent<StaticMineModuleComponent>();
        return entity;
    }

    int GenerateIndex() => Battle.MineProcessor.GenerateIndex();
}
