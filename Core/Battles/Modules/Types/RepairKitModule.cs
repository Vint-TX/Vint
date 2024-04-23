using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class RepairKitModule : ActiveBattleModule, IHealthModule {
    public override string ConfigPath => "garage/module/upgrade/properties/repairkit";

    public override bool ActivationCondition => Tank.Health < Tank.MaxHealth;

    public override void Activate() {
        if (!CanBeActivated) return;

        base.Activate();
        RepairKitEffect? effect = Tank.Effects.OfType<RepairKitEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                extendableEffect.Extend(Level);
                break;
        }
    }

    public override RepairKitEffect GetEffect() => new(Tank, Level);

    public override void TryUnblock() {
        if (Tank.Health >= Tank.MaxHealth) return;

        base.TryUnblock();
    }

    public override void TryBlock(bool force = false, long blockTimeMs = 0) {
        if (!force && Tank.Health < Tank.MaxHealth) return;

        base.TryBlock(force, blockTimeMs);
    }
    
    public void OnHealthChanged(float before, float current, float max) {
        if (current < max) TryUnblock();
        else TryBlock(true);
    }
}