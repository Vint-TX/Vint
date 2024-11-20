using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Tank;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Components.Server.Modules.Effect.Healing;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.Modules.Effect.Common.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class RepairKitEffect : DurationEffect, ISupplyEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/healing";
    const string MarketConfigPath = "garage/module/upgrade/properties/repairkit";

    public RepairKitEffect(BattleTank tank, int level = -1) : base(tank, level, MarketConfigPath) {
        SupplyHealingComponent = ConfigManager.GetComponent<HealingComponent>(EffectConfigPath);

        InstantHp = IsSupply ? 0 : Leveling.GetStat<ModuleHealingEffectInstantHPPropertyComponent>(MarketConfigPath, Level);
        Percent = IsSupply ? SupplyHealingComponent.Percent : Leveling.GetStat<ModuleHealingEffectPercentPropertyComponent>(MarketConfigPath, Level);
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration * Tank.SupplyDurationMultiplier;
        TickPeriod = TimeSpan.FromMilliseconds(ConfigManager.GetComponent<TickComponent>(EffectConfigPath).Period);

        if (IsSupply)
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);

        Heal = HealLeft = Tank.MaxHealth * Percent;
        HealPerMs = (float)(Heal / Duration.TotalMilliseconds);
    }

    HealingComponent SupplyHealingComponent { get; }

    float InstantHp { get; set; }
    float Percent { get; set; }
    TimeSpan TickPeriod { get; }

    DateTimeOffset LastTick { get; set; }
    TimeSpan TimePassedFromLastTick => DateTimeOffset.UtcNow - LastTick;

    float Heal { get; set; }
    float HealLeft { get; set; }
    float HealPerMs { get; set; }

    public async Task Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        LastTick = DateTimeOffset.UtcNow.AddTicks(-TickPeriod.Ticks);

        bool isSupply = newLevel < 0;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            Percent = SupplyHealingComponent.Percent;
        } else {
            Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(MarketConfigPath, newLevel));
            InstantHp = Leveling.GetStat<ModuleHealingEffectInstantHPPropertyComponent>(MarketConfigPath, newLevel);
            Percent = Leveling.GetStat<ModuleHealingEffectPercentPropertyComponent>(MarketConfigPath, newLevel);

            CalculatedDamage heal = new(default, InstantHp, false, false);
            await Battle.DamageProcessor.Heal(Tank, heal);
        }

        Level = newLevel;

        Heal = HealLeft = Tank.MaxHealth * Percent;
        HealPerMs = (float)(Heal / Duration.TotalMilliseconds);

        await Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        await Entity!.RemoveComponent<DurationComponent>();
        await Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float SupplyMultiplier => 0;
    public float SupplyDurationMs { get; }

    public override async Task Activate() {
        if (IsActive) return;

        LastTick = DateTimeOffset.UtcNow.AddTicks(-TickPeriod.Ticks);
        Tank.Effects.Add(this);

        CalculatedDamage heal = new(default, InstantHp, false, false);
        await Battle.DamageProcessor.Heal(Tank, heal);

        Entity = new HealingEffectTemplate().Create(Tank.BattlePlayer, Duration);
        await ShareToAllPlayers();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;

        if (HealLeft <= 0 || Tank.Health >= Tank.MaxHealth) return;

        CalculatedDamage heal = new(default, HealLeft, false, false);
        await Battle.DamageProcessor.Heal(Tank, heal);
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        TimeSpan timePassed = TimePassedFromLastTick;

        if (!IsActive || HealLeft <= 0 || timePassed < TickPeriod) return;

        LastTick = DateTimeOffset.UtcNow;

        float healHp = Math.Min(Math.Min((float)(timePassed.TotalMilliseconds * HealPerMs), Tank.MaxHealth - Tank.Health), HealLeft);
        HealLeft -= healHp;

        if (Tank.Health >= Tank.MaxHealth) return;

        CalculatedDamage heal = new(default, healHp, false, false);
        await Battle.DamageProcessor.Heal(Tank, heal);
    }
}
