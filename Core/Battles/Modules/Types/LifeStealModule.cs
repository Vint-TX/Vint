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

    float FixedHeal { get; set; }
    float HpPercent { get; set; }
    float HpFromCurrentTank { get; set; }
    float Heal { get; set; }

    public override LifeStealEffect GetEffect() => new(Tank, Level);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        LifeStealEffect? effect = Tank.Effects.OfType<LifeStealEffect>().SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        await effect.Activate();

        await base.Activate();
        IEntity effectEntity = effect.Entity!;
        Battle battle = Tank.Battle;

        CalculatedDamage heal = new(default, Heal, false, false);
        await battle.DamageProcessor.Heal(Tank, heal);

        foreach (BattlePlayer player in battle.Players.Where(player => player.InBattle))
            await player.PlayerConnection.Send(new TriggerEffectExecuteEvent(), effectEntity);
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        FixedHeal = Leveling.GetStat<ModuleLifestealEffectFixedHPPropertyComponent>(ConfigPath, Level);
        HpPercent = Leveling.GetStat<ModuleLifestealEffectAdditiveHPFactorPropertyComponent>(ConfigPath, Level);
        HpFromCurrentTank = Tank.MaxHealth * HpPercent;
        Heal = FixedHeal + HpFromCurrentTank;
    }

    public Task OnKill(BattleTank target) => Activate();
}
