using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Templates.Battle.Effect;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class TurboSpeedEffect : DurationEffect, ISupplyEffect, IExtendableEffect {
    const string EffectConfigPath = "battle/effect/turbospeed";
    const string MarketConfigPath = "garage/module/upgrade/properties/turbospeed";

    public TurboSpeedEffect(BattleTank tank, int level = -1) : base(tank, level, MarketConfigPath) {
        MultipliersComponent = ConfigManager.GetComponent<ModuleTurbospeedEffectPropertyComponent>(MarketConfigPath);

        SupplyMultiplier = ConfigManager.GetComponent<TurboSpeedEffectComponent>(EffectConfigPath).SpeedCoeff;
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration * Tank.SupplyDurationMultiplier;

        Multiplier = IsSupply ? SupplyMultiplier : MultipliersComponent.UpgradeLevel2Values[Level];

        if (IsSupply)
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
    }

    ModuleTurbospeedEffectPropertyComponent MultipliersComponent { get; }

    public void Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        bool isSupply = newLevel < 0;

        float newMultiplier;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            newMultiplier = SupplyMultiplier;
        } else {
            Duration = TimeSpan.FromMilliseconds(DurationsComponent.UpgradeLevel2Values[newLevel]);
            newMultiplier = MultipliersComponent.UpgradeLevel2Values[newLevel];
        }

        float additiveMultiplier = newMultiplier / Multiplier;
        Multiplier = newMultiplier;
        Tank.Tank.ChangeComponent<SpeedComponent>(component => component.Speed *= additiveMultiplier);

        Level = newLevel;

        Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        Entity!.RemoveComponent<DurationComponent>();
        Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float Multiplier { get; private set; }

    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }

    public override void Activate() {
        if (IsActive) return;

        Tank.Effects.Add(this);

        Entities.Add(new TurboSpeedEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();

        Tank.OriginalSpeedComponent.Speed *= Multiplier;
        Tank.UpdateSpeed();
        Schedule(Duration, Deactivate);
    }

    public override void Deactivate() {
        if (!IsActive) return;

        Tank.Effects.TryRemove(this);

        UnshareAll();
        Entities.Clear();

        Tank.OriginalSpeedComponent.Speed /= Multiplier;
        Tank.UpdateSpeed();
    }
}
