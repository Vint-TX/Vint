using Vint.Core.Battle.Effects.Components.Config.LifeSteal;
using Vint.Core.Battle.Effects.Impl;
using Vint.Core.Battle.Modules.Common.Events;
using Vint.Core.Battle.Modules.Impl.Base;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons.Damage.Calculator;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;

namespace Vint.Core.Battle.Modules.Impl;

[ModuleId(-246333323)]
public class LifeStealModule : BattleModule, IKillModule {
    public override string ConfigPath => "garage/module/upgrade/properties/lifesteal";

    protected override bool ActivationCondition => Tank.Health < Tank.MaxHealth;

    float FixedHeal { get; set; }
    float HpPercent { get; set; }
    float HpFromCurrentTank { get; set; }
    float Heal { get; set; }

    public Task OnKill(BattleTank target) => Activate();

    public override LifeStealEffect GetEffect() => new(Tank, Level);

    public override async Task Activate() {
        if (!CanBeActivated) return;

        LifeStealEffect? effect = Tank.Effects
            .OfType<LifeStealEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        effect = GetEffect();
        await effect.Activate();

        await base.Activate();
        IEntity effectEntity = effect.Entity!;

        CalculatedDamage heal = new(default, Heal, false, false);
        await Round.DamageProcessor.Heal(Tank, heal);

        await Round.Players.Send<TriggerEffectExecuteEvent>(effectEntity);
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        FixedHeal = GetStat<ModuleLifestealEffectFixedHPPropertyComponent>();
        HpPercent = GetStat<ModuleLifestealEffectAdditiveHPFactorPropertyComponent>();
        HpFromCurrentTank = Tank.MaxHealth * HpPercent;
        Heal = FixedHeal + HpFromCurrentTank;
    }
}
