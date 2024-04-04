using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Templates.Battle.Effect;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class AbsorbingArmorEffect : Effect, ISupplyEffect, IDamageEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/armor";

    public AbsorbingArmorEffect(BattleTank tank, int level = -1) : base(tank, level) {
        MultipliersComponent = ConfigManager.GetComponent<ModuleArmorEffectPropertyComponent>(ConfigPath);
        DurationsComponent = ConfigManager.GetComponent<ModuleEffectDurationPropertyComponent>(ConfigPath);

        SupplyMultiplier = ConfigManager.GetComponent<ArmorEffectComponent>(EffectConfigPath).Factor;
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration;

        Multiplier = IsSupply ? SupplyMultiplier : MultipliersComponent[Level];
        Duration = IsSupply ? TimeSpan.FromMilliseconds(SupplyDurationMs) : TimeSpan.FromMilliseconds(DurationsComponent[Level]);
    }

    public override string ConfigPath => "garage/module/upgrade/properties/absorbingarmor";
    ModuleArmorEffectPropertyComponent MultipliersComponent { get; }
    public float Multiplier { get; private set; }

    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash) =>
        IsActive &&
        Tank == target &&
        (source.WeaponHandler is not IsisWeaponHandler ||
         source.IsEnemy(target)) ? Multiplier : 1;

    public ModuleEffectDurationPropertyComponent DurationsComponent { get; }

    public void Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        bool isSupply = newLevel < 0;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            Multiplier = SupplyMultiplier;
        } else {
            Duration = TimeSpan.FromMilliseconds(DurationsComponent[newLevel]);
            Multiplier = MultipliersComponent[newLevel];
        }

        Level = newLevel;
        LastActivationTime = DateTimeOffset.UtcNow;

        Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        Entity!.RemoveComponent<DurationComponent>();
        Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }

    public override void Activate() {
        if (IsActive) return;

        base.Activate();

        Entities.Add(new ArmorEffectTemplate().Create(Tank.BattlePlayer, Duration));
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
}