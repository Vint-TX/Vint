using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Module;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class RageModule : TriggerBattleModule, IKillModule {
    public override string ConfigPath => "garage/module/upgrade/properties/rage";

    public override RageEffect GetEffect() => new(DecreaseCooldownPerKill, Tank, Level);

    TimeSpan DecreaseCooldownPerKill { get; set; }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        DecreaseCooldownPerKill =
            TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleRageEffectReduceCooldownTimePerKillPropertyComponent>(ConfigPath, Level));
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        RageEffect? effect = Tank.Effects.OfType<RageEffect>().SingleOrDefault();

        if (effect != null) return;

        foreach (BattleModule module in Tank.Modules) {
            if (module.StateManager.CurrentState is not Cooldown cooldown) continue;

            cooldown.AddElapsedTime(DecreaseCooldownPerKill);
        }

        effect = GetEffect();
        await effect.Activate();

        await Tank.BattlePlayer.PlayerConnection.Send(new TriggerEffectExecuteEvent(), effect.Entity!);
        await base.Activate();
    }

    public Task OnKill(BattleTank target) => Activate();
}
