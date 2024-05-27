using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Modules.Types;

public class InvisibilityModule : ActiveBattleModule, IFlagModule, IShotModule {
    public override string ConfigPath => "garage/module/upgrade/properties/invisibility";

    public override InvisibilityEffect GetEffect() => new(Duration, Tank, Level);

    public override bool ActivationCondition => Effect == null;

    TimeSpan Duration { get; set; }

    InvisibilityEffect? Effect { get; set; }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        Effect = GetEffect();
        Effect.Deactivated += Deactivated;
        await Effect.Activate();
        await base.Activate();
    }

    public override async Task Init(BattleTank tank, IEntity userSlot, IEntity marketModule) {
        await base.Init(tank, userSlot, marketModule);

        Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(ConfigPath, Level));
    }

    public async Task OnFlagAction(FlagAction action) {
        if (action == FlagAction.Capture)
            await TryDeactivate();
    }

    public Task OnShot() => TryDeactivate();

    Task TryDeactivate() =>
        Effect == null
            ? Task.CompletedTask
            : Effect.Deactivate();

    void Deactivated() => Effect = null;
}
