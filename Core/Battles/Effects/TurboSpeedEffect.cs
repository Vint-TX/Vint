using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Effect;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Server;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Templates.Battle.Effect;
using Vint.Core.Utils;
using DurationComponent = Vint.Core.ECS.Components.Battle.Effect.DurationComponent;
using EffectDurationComponent = Vint.Core.ECS.Components.Server.DurationComponent;

namespace Vint.Core.Battles.Effects;

public sealed class TurboSpeedEffect : Effect, ISupplyEffect, IExtendableEffect, ISpeedEffect {
    const string EffectConfigPath = "battle/effect/turbospeed";

    public TurboSpeedEffect(BattleTank tank, int level = -1) : base(tank, level) {
        DurationsComponent = ConfigManager.GetComponent<ModuleEffectDurationPropertyComponent>(ConfigPath);
        MultipliersComponent = ConfigManager.GetComponent<ModuleTurbospeedEffectPropertyComponent>(ConfigPath);

        SupplyMultiplier = ConfigManager.GetComponent<TurboSpeedEffectComponent>(EffectConfigPath).SpeedCoeff;
        SupplyDurationMs = ConfigManager.GetComponent<EffectDurationComponent>(EffectConfigPath).Duration;

        Multiplier = IsSupply ? SupplyMultiplier : MultipliersComponent[Level];
        Duration = IsSupply ? TimeSpan.FromMilliseconds(SupplyDurationMs) : TimeSpan.FromMilliseconds(DurationsComponent[Level]);
    }

    public override string ConfigPath => "garage/module/upgrade/properties/turbospeed";
    ModuleTurbospeedEffectPropertyComponent MultipliersComponent { get; }
    SpeedComponent SpeedComponentWithEffect { get; set; } = null!;

    public ModuleEffectDurationPropertyComponent DurationsComponent { get; }

    public void Extend(int newLevel) {
        if (!IsActive) return;

        UnScheduleAll();

        bool isSupply = newLevel < 0;

        float newMultiplier;

        if (isSupply) {
            Duration = TimeSpan.FromMilliseconds(SupplyDurationMs);
            newMultiplier = SupplyMultiplier;
        } else {
            Duration = TimeSpan.FromMilliseconds(DurationsComponent[newLevel]);
            newMultiplier = MultipliersComponent[newLevel];
        }

        float additiveMultiplier = newMultiplier / Multiplier;
        Multiplier = newMultiplier;
        Tank.Tank.ChangeComponent<SpeedComponent>(component => component.Speed *= additiveMultiplier);

        Level = newLevel;
        LastActivationTime = DateTimeOffset.UtcNow;

        Entity!.ChangeComponent<DurationConfigComponent>(component => component.Duration = Convert.ToInt64(Duration.TotalMilliseconds));
        Entity!.RemoveComponent<DurationComponent>();
        Entity!.AddComponent(new DurationComponent(DateTimeOffset.UtcNow));

        Schedule(Duration, Deactivate);
    }

    public float Multiplier { get; private set; }

    public void UpdateTankSpeed() {
        if (!IsActive) return;

        if (Tank.Temperature < 0) {
            float minTemperature = Tank.TemperatureConfig.MinTemperature;

            float newSpeed = MathUtils.Map(Tank.Temperature, 0, minTemperature, SpeedComponentWithEffect.Speed, 0);
            float newTurnSpeed = MathUtils.Map(Tank.Temperature, 0, minTemperature, SpeedComponentWithEffect.TurnSpeed, 0);
            float newWeaponSpeed = MathUtils.Map(Tank.Temperature, 0, minTemperature, Tank.WeaponHandler.OriginalWeaponRotationComponent.Speed, 0);

            Tank.Tank.ChangeComponent<SpeedComponent>(component => {
                component.Speed = newSpeed;
                component.TurnSpeed = newTurnSpeed;
            });
            Tank.Weapon.ChangeComponent<WeaponRotationComponent>(component => component.Speed = newWeaponSpeed);
        } else {
            Tank.Tank.ChangeComponent(SpeedComponentWithEffect.Clone());
            Tank.Weapon.ChangeComponent(Tank.WeaponHandler.OriginalWeaponRotationComponent.Clone());
        }
    }

    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }

    public override void Activate() {
        if (IsActive) return;

        base.Activate();

        Entities.Add(new TurboSpeedEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();

        Tank.Tank.ChangeComponent<SpeedComponent>(component => component.Speed *= Multiplier);
        LastActivationTime = DateTimeOffset.UtcNow;
        SpeedComponentWithEffect = Tank.Tank.GetComponent<SpeedComponent>().Clone();

        Schedule(Duration, Deactivate);
    }

    public override void Deactivate() {
        if (!IsActive) return;

        base.Deactivate();

        UnshareAll();
        Entities.Clear();

        Tank.Tank.ChangeComponent<SpeedComponent>(component => component.Speed /= Multiplier);
        LastActivationTime = default;
    }
}