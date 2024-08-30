using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Server.Modules.Effect.EmergencyProtection;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;

namespace Vint.Core.Battles.Modules.Types;

[ModuleId(-357196071)]
public class EmergencyProtectionModule : TriggerBattleModule, IHealthModule, ITemperatureWeaponHandler {
    public override string ConfigPath => "garage/module/upgrade/properties/emergencyprotection";

    public override EmergencyProtectionEffect GetEffect() => new(Duration, Tank, Level);

    float AdditiveHpFactor { get; set; }
    float FixedHp { get; set; }
    TimeSpan Duration { get; set; }

    CalculatedDamage CalculatedHeal => new(default, Tank.MaxHealth * AdditiveHpFactor + FixedHp, false, false);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        EmergencyProtectionEffect? effect = Tank.Effects.OfType<EmergencyProtectionEffect>().SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        await effect.Activate();

        IEntity effectEntity = effect.Entity!;

        await base.Activate();

        // todo reset tank temperature

        await Battle.DamageProcessor.Heal(Tank, CalculatedHeal);
        // todo freeze the tank

        foreach (BattlePlayer player in Battle.Players.Where(player => player.InBattle))
            await player.PlayerConnection.Send(new TriggerEffectExecuteEvent(), effectEntity);
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        AdditiveHpFactor = GetStat<ModuleEmergencyProtectionEffectAdditiveHPFactorPropertyComponent>();
        FixedHp = GetStat<ModuleEmergencyProtectionEffectFixedHPPropertyComponent>();
        Duration = TimeSpan.FromMilliseconds(GetStat<ModuleEmergencyProtectionEffectHolyshieldDurationPropertyComponent>());
    }

    public async Task OnHealthChanged(float before, float current, float max) {
        if (current > 0) return;

        await Activate();
    }

    public IEntity BattleEntity => Entity;
    public float TemperatureLimit => -1f;
    public float TemperatureDelta => -1f;
    public TimeSpan TemperatureDuration => TimeSpan.Zero;
}
