using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class EmergencyProtectionModule : TriggerBattleModule, IHealthModule, ITemperatureWeaponHandler {
    public override string ConfigPath => "garage/module/upgrade/properties/emergencyprotection";

    public override EmergencyProtectionEffect GetEffect() => new(Duration, Tank, Level);

    float AdditiveHpFactor { get; set; }
    float FixedHp { get; set; }
    TimeSpan Duration { get; set; }

    CalculatedDamage CalculatedHeal => new(default, Tank.MaxHealth * AdditiveHpFactor + FixedHp, false, false);

    public override void Activate() {
        if (!CanBeActivated) return;

        EmergencyProtectionEffect? effect = Tank.Effects.OfType<EmergencyProtectionEffect>().SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        effect.Activate();

        IEntity effectEntity = effect.Entity!;

        base.Activate();

        Battle battle = Tank.Battle;

        Tank.TemperatureAssists.Clear();

        battle.DamageProcessor.Heal(Tank, CalculatedHeal);
        Tank.UpdateTemperatureAssists(Tank, this, false);

        foreach (BattlePlayer player in battle.Players.Where(player => player.InBattle))
            player.PlayerConnection.Send(new TriggerEffectExecuteEvent(), effectEntity);
    }

    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);

        AdditiveHpFactor = Leveling.GetStat<ModuleEmergencyProtectionEffectAdditiveHPFactorPropertyComponent>(ConfigPath, Level);
        FixedHp = Leveling.GetStat<ModuleEmergencyProtectionEffectFixedHPPropertyComponent>(ConfigPath, Level);
        Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEmergencyProtectionEffectHolyshieldDurationPropertyComponent>(ConfigPath, Level));
    }

    public void OnHealthChanged(float before, float current, float max) {
        if (current > 0) return;

        Activate();
    }

    public IEntity BattleEntity => Entity;
    public float TemperatureLimit => -0.99f;
    public float TemperatureDelta => -0.99f;
}
