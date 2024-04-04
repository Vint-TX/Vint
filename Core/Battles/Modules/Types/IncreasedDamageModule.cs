using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Modules.Types.Base;

namespace Vint.Core.Battles.Modules.Types;

public class IncreasedDamageModule : ActiveBattleModule {
    public override string ConfigPath => "garage/module/upgrade/properties/increaseddamage";

    public override void Activate() {
        if (!CanBeActivated) return;

        base.Activate();
        IncreasedDamageEffect? effect = Tank.Effects.OfType<IncreasedDamageEffect>().SingleOrDefault();

        switch (effect) {
            case null:
                GetEffect().Activate();
                break;

            case IExtendableEffect extendableEffect:
                extendableEffect.Extend(Level);
                break;
        }
    }

    public override IncreasedDamageEffect GetEffect() => new(Tank, Level);
}