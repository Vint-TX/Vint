using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Components.Server.Modules.Effect.IncreasedDamage;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.Modules.Effect.Common.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class IncreasedDamageEffect : DurationEffect, ISupplyEffect, IDamageMultiplierEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/damage";
    const string MarketConfigPath = "garage/module/upgrade/properties/increaseddamage";

    public IncreasedDamageEffect(BattleTank tank, int level = -1) : base(tank, level, MarketConfigPath) {
        SupplyMultiplier = ConfigManager.GetComponent<DamageEffectComponent>(EffectConfigPath).Factor;
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration * Tank.SupplyDurationMultiplier;

        Multiplier = IsSupply ? SupplyMultiplier : Leveling.GetStat<ModuleDamageEffectMaxFactorPropertyComponent>(MarketConfigPath, Level);

        if (IsSupply)
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
    }

    public float Multiplier { get; private set; }

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) =>
        IsActive && (Tank != target || isSplash) ? Multiplier : 1;

    public async Task Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        bool isSupply = newLevel < 0;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            Multiplier = SupplyMultiplier;
        } else {
            Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(MarketConfigPath, newLevel));
            Multiplier = Leveling.GetStat<ModuleDamageEffectMaxFactorPropertyComponent>(MarketConfigPath, newLevel);
        }

        Level = newLevel;

        await Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Duration);
        await Entity!.RemoveComponent<DurationComponent>();
        await Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entity = new DamageEffectTemplate().Create(Tank.BattlePlayer, Duration);
        await ShareToAllPlayers();

        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareFromAllPlayers();
        Entity = null;
    }
}
