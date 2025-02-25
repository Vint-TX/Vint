using Vint.Core.Battle.Effects;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;

namespace Vint.Core.Battle.Modules.Types;

[ModuleId(-862259125)]
public class RepairKitModule : ActiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/repairkit";

    protected override bool ActivationCondition => Tank.Health < Tank.MaxHealth;

    public async Task OnHealthChanged(float before, float current, float max) {
        if (current < max) await TryUnblock();
        else await TryBlock();
    }

    public override async Task Activate() {
        if (!CanBeActivated) return;

        await base.Activate();

        RepairKitEffect? effect = Tank
            .Effects
            .OfType<RepairKitEffect>()
            .SingleOrDefault();

        switch (effect) {
            case null:
                await GetEffect()
                    .Activate();

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
}
