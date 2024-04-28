using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class LifeStealModule : BattleModule, IKillModule {
    public override string ConfigPath => "garage/module/upgrade/properties/lifesteal";

    public override bool ActivationCondition => Tank.Health < Tank.MaxHealth;

    BattleTank KilledTank { get; set; } = null!;
    float FixedHeal { get; set; }
    float HpPercent { get; set; }

    public override LifeStealEffect GetEffect() => new(Tank, Level);

    public override void Activate() {
        if (!CanBeActivated) return;

        LifeStealEffect? effect = Tank.Effects.OfType<LifeStealEffect>().SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        effect.Activate();

        IEntity effectEntity = effect.Entity!;

        base.Activate();

        Battle battle = Tank.Battle;

        float stolenHp = KilledTank.MaxHealth * HpPercent;
        CalculatedDamage heal = new(default, FixedHeal + stolenHp, false, false);

        battle.DamageProcessor.Heal(Tank, heal);

        foreach (BattlePlayer player in battle.Players.Where(player => player.InBattle))
            player.PlayerConnection.Send(new TriggerEffectExecuteEvent(), effectEntity);
    }

    public override void Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        base.Init(tank, userSlot, marketModule);

        FixedHeal = Leveling.GetStat<ModuleLifestealEffectFixedHPPropertyComponent>(ConfigPath, Level);
        HpPercent = Leveling.GetStat<ModuleLifestealEffectAdditiveHPFactorPropertyComponent>(ConfigPath, Level);
    }

    public void OnKill(BattleTank target) {
        KilledTank = target;
        Activate();
        KilledTank = null!;
    }
}
