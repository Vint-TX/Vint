using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Server.Modules.Effect.Rage;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(1215656773)]
public class RageModule : TriggerBattleModule, IKillModule {
    public override string ConfigPath => "garage/module/upgrade/properties/rage";

    TimeSpan DecreaseCooldownPerKill { get; set; }

    public Task OnKill(BattleTank target) => Activate();

    public override RageEffect GetEffect() => new(Tank, Level);

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        DecreaseCooldownPerKill = TimeSpan.FromMilliseconds(GetStat<ModuleRageEffectReduceCooldownTimePerKillPropertyComponent>());
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        RageEffect? effect = Tank
            .Effects
            .OfType<RageEffect>()
            .SingleOrDefault();

        if (effect != null) return;

        foreach (BattleModule module in Tank.Modules) {
            if (module.StateManager.CurrentState is not Cooldown cooldown) continue;

            cooldown.AddElapsedTime(DecreaseCooldownPerKill);
        }

        effect = GetEffect();
        await effect.Activate();

        await Tank.Tanker.Connection.Send(new TriggerEffectExecuteEvent(), effect.Entity!);
        await base.Activate();
    }
}
