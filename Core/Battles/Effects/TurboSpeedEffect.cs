using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class TurboSpeedEffect : DurationEffect, ISupplyEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/turbospeed";
    const string MarketConfigPath = "garage/module/upgrade/properties/turbospeed";

    public TurboSpeedEffect(BattleTank tank, int level = -1) : base(tank, level, MarketConfigPath) {
        SupplyMultiplier = ConfigManager.GetComponent<TurboSpeedEffectComponent>(EffectConfigPath).SpeedCoeff;
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration * Tank.SupplyDurationMultiplier;

        Multiplier = IsSupply ? SupplyMultiplier : Leveling.GetStat<ModuleTurbospeedEffectPropertyComponent>(MarketConfigPath, Level);

        if (IsSupply)
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
    }

    public async Task Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        bool isSupply = newLevel < 0;

        float newMultiplier;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            newMultiplier = SupplyMultiplier;
        } else {
            Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(MarketConfigPath, newLevel));
            newMultiplier = Leveling.GetStat<ModuleTurbospeedEffectPropertyComponent>(MarketConfigPath, newLevel);
        }

        float additiveMultiplier = newMultiplier / Multiplier;
        Multiplier = newMultiplier;
        await Tank.Tank.ChangeComponent<SpeedComponent>(component => component.Speed *= additiveMultiplier);

        Level = newLevel;

        await Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        await Entity!.RemoveComponent<DurationComponent>();
        await Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float Multiplier { get; private set; }

    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }

    public override async Task Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new TurboSpeedEffectTemplate().Create(Tank.BattlePlayer, Duration));
        await ShareAll();

        Tank.OriginalSpeedComponent.Speed *= Multiplier;
        await Tank.UpdateSpeed();
        Schedule(Duration, Deactivate);
    }

    public override async Task Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        await UnshareAll();
        Entities.Clear();

        Tank.OriginalSpeedComponent.Speed /= Multiplier;
        await Tank.UpdateSpeed();
    }
}
