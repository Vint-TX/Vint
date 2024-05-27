using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class RepairKitModule : ActiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/repairkit";

    public override bool ActivationCondition => Tank.Health < Tank.MaxHealth;

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();
        RepairKitEffect? effect = Tank.Effects.OfType<RepairKitEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                await extendableEffect.Extend(Level);
                break;
        }
    }

    public override RepairKitEffect GetEffect() => new(Tank, Level);

    public override async Task TryUnblock() {
        if (Tank.Health >= Tank.MaxHealth) return;

        await base.TryUnblock();
    }

    public override async Task TryBlock(bool force = false, long blockTimeMs = 0) {
        if (!force && Tank.Health < Tank.MaxHealth) return;

        await base.TryBlock(force, blockTimeMs);
    }

    public async Task OnHealthChanged(float before, float current, float max) {
        if (current < max) await TryUnblock();
        else await TryBlock(true);
    }
}
