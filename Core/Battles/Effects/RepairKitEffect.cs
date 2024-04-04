using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Templates.Battle.Effect;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class RepairKitEffect : Effect, ISupplyEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/healing";

    public RepairKitEffect(BattleTank tank, int level = -1) : base(tank, level) {
        DurationsComponent = ConfigManager.GetComponent<ModuleEffectDurationPropertyComponent>(ConfigPath);
        HpPerMsComponent = ConfigManager.GetComponent<ModuleHealingEffectHPPerMSPropertyComponent>(ConfigPath);
        InstantHpComponent = ConfigManager.GetComponent<ModuleHealingEffectInstantHPPropertyComponent>(ConfigPath);
        SupplyHealingComponent = ConfigManager.GetComponent<HealingComponent>(EffectConfigPath);

        InstantHp = IsSupply ? 0 : InstantHpComponent[Level];
        HpPerMs = IsSupply ? SupplyHealingComponent.HpPerMs : HpPerMsComponent[Level];
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration;
        TickPeriod = TimeSpan.FromMilliseconds(ConfigManager.GetComponent<TickComponent>(EffectConfigPath).Period);

        Duration = IsSupply ? TimeSpan.FromMilliseconds(SupplyDurationMs) : TimeSpan.FromMilliseconds(DurationsComponent[Level]);
    }

    public override string ConfigPath => "garage/module/upgrade/properties/repairkit";
    ModuleHealingEffectHPPerMSPropertyComponent HpPerMsComponent { get; }
    ModuleHealingEffectInstantHPPropertyComponent InstantHpComponent { get; }
    HealingComponent SupplyHealingComponent { get; }

    float InstantHp { get; set; }
    float HpPerMs { get; set; }
    TimeSpan TickPeriod { get; }

    DateTimeOffset LastTick { get; set; }
    TimeSpan TimePassedFromLastTick => DateTimeOffset.UtcNow - LastTick;

    public ModuleEffectDurationPropertyComponent DurationsComponent { get; }

    public void Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        LastTick = DateTimeOffset.UtcNow.AddTicks(-TickPeriod.Ticks);

        bool isSupply = newLevel < 0;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            HpPerMs = SupplyHealingComponent.HpPerMs;
        } else {
            Duration = TimeSpan.FromMilliseconds(DurationsComponent[newLevel]);
            InstantHp = InstantHpComponent[newLevel];
            HpPerMs = HpPerMsComponent[newLevel];

            CalculatedDamage heal = new(default, InstantHp, false, false, false, false);
            Battle.DamageProcessor.Heal(Tank, heal);
        }

        Level = newLevel;
        LastActivationTime = DateTimeOffset.UtcNow;

        Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        Entity!.RemoveComponent<DurationComponent>();
        Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float SupplyMultiplier => 0;
    public float SupplyDurationMs { get; }

    public override void Activate() {
        if (IsActive) return;

        base.Activate();

        CalculatedDamage heal = new(default, InstantHp, false, false, false, false);

        LastTick = DateTimeOffset.UtcNow.AddTicks(-TickPeriod.Ticks);
        Battle.DamageProcessor.Heal(Tank, heal);

        Entities.Add(new HealingEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();

        LastActivationTime = DateTimeOffset.UtcNow;

        Schedule(Duration, Deactivate);
    }

    public override void Deactivate() {
        if (!IsActive) return;

        base.Deactivate();

        UnshareAll();
        Entities.Clear();

        LastActivationTime = default;
    }

    public override void Tick() {
        base.Tick();

        TimeSpan timePassed = TimePassedFromLastTick;

        if (!IsActive || timePassed < TickPeriod) return;

        LastTick = DateTimeOffset.UtcNow;

        if (Tank.Health >= Tank.MaxHealth) return;

        float healHp = Math.Min(Convert.ToSingle(timePassed.TotalMilliseconds * HpPerMs), Tank.MaxHealth - Tank.Health);
        CalculatedDamage heal = new(default, healHp, false, false, false, false);

        Battle.DamageProcessor.Heal(Tank, heal);
    }
}